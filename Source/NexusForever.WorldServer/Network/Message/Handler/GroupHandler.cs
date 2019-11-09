using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System;

#nullable enable

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite request)
        {
            FindPlayer(session, request.PlayerName, GroupInviteType.Invite, 0, targetPlayer =>
            {
                var player = session.Player;
                var group = player.GroupMember?.Group ?? GlobalGroupManager.CreateGroup(player);
                group.Invite(player, targetPlayer);
            });
        }

        [MessageHandler(GameMessageOpcode.ClientGroupRequestJoin)]
        public static void HandleClientGroupRequestJoin(WorldSession session, ClientGroupRequestJoin request)
        {
            var groupId = session.Player.GroupMember?.Group.Id ?? 0;
            var type = groupId == 0
                     ? GroupInviteType.Request
                     : GroupInviteType.Referral;

            FindPlayer(session, request.PlayerName, type, groupId, targetPlayer =>
            {
                if (targetPlayer.GroupMember?.Group is Group joinGroup)
                {
                    joinGroup.RequestJoin(targetPlayer, session.Player);
                    return;
                }
                
                if (session.Player.GroupMember?.Group is Group referGroup)
                {
                    referGroup.RequestReferral(session.Player, targetPlayer);
                    return;
                }
                
                // should this be invite to group instead?
                session.EnqueueMessageEncrypted(new ServerGroupRequestJoinResult
                {
                    GroupId = 0,
                    PlayerName = request.PlayerName,
                    Result = InviteResult.GroupNotFound,
                    IsJoin = type == GroupInviteType.Request
                });
            });
        }

        [MessageHandler(GameMessageOpcode.ClientGroupRequestJoinResponse)]
        public static void HandleClientGroupRequestJoinResponse(WorldSession session, ClientGroupRequestJoinResponse request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            var referral = group.FindReferral(request.PlayerName);
            var type = referral is null
                     ? GroupInviteType.Request
                     : GroupInviteType.Referral;

            FindPlayer(session, request.PlayerName, type, group.Id, targetPlayer =>
            {
                if (referral != null)
                {
                    if (referral.Player.CharacterId != targetPlayer.CharacterId)
                        throw new InvalidPacketValueException();

                    group.HandleRequestReferral(referral, request.Accepted);
                    return;
                }

                var invite = targetPlayer.GroupInvite;
                if (invite != null)
                {
                    group.HandleRequestJoin(invite, request.Accepted);
                    return;
                }
            });
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse request)
        {
            var player = session.Player;
            
            var invite = player.GroupInvite;
            if (invite is null)
                return;

            invite.Group.HandleInvite(invite, request.Response);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.Leave(player, request.Scope);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupPromote)]
        public static void HandleGroupPromote(WorldSession session, ClientGroupPromote request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.Promote(player, request.PlayerIdentity);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSetRole)]
        public static void HandleGroupSetRole(WorldSession session, ClientGroupSetRole request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.UpdateMemberFlags(player, request.PlayerIdentity, request.ChangedFlags, request.Flags);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSendReadyCheck)]
        public static void HandleGroupSendReadyCheck(WorldSession session, ClientGroupSendReadyCheck request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.ReadyCheck(player, request.Message);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupFlagsChanged)]
        public static void HandleGroupFlagsChanged(WorldSession session, ClientGroupFlags request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.UpdatePartyFlags(player, request.Flag);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupKick)]
        public static void HandleGroupKick(WorldSession session, ClientGroupKick request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.Kick(player, request.PlayerIdentity);
        }

        /// <summary>
        /// Validate that current player is in a group with given group ID
        /// </summary>
        /// <returns>Tuple containing GroupMember and Group objects</returns>
        private static (Group, Player) ValidateGroupMembership(WorldSession session, ulong groupId)
        {
            var player = session.Player;
            var member = player.GroupMember;
            if (member is null)
                throw new InvalidPacketValueException();

            var group = member.Group;
            if (group is null || group.Id != groupId)
                throw new InvalidPacketValueException();

            return (group, player);
        }
        
        /// <summary>
        /// Find player by name and get their session
        /// </summary>
        /// <param name="session">Requesting player's session</param>
        /// <param name="playerName">Player to find</param>
        /// <param name="type">Type of invite</param>
        /// <param name="handler">Callback to execute</param>
        private static void FindPlayer(WorldSession session, string playerName, GroupInviteType type, ulong groupId, Action<Player> handler)
        {
            void sendNotFound()
            {
                IWritable message = type switch
                {
                    GroupInviteType.Invite => new ServerGroupInviteResult
                    {
                        GroupId = groupId,
                        PlayerName = playerName,
                        Result = InviteResult.PlayerNotFound
                    },
                    GroupInviteType.Request => new ServerGroupRequestJoinResult
                    {
                        GroupId = groupId,
                        PlayerName = playerName,
                        Result = InviteResult.PlayerNotFound,
                        IsJoin = true
                    },
                    GroupInviteType.Referral => new ServerGroupRequestJoinResult
                    {
                        GroupId = groupId,
                        PlayerName = playerName,
                        Result = InviteResult.PlayerNotFound,
                        IsJoin = false
                    },
                    _ => throw new InvalidOperationException()
                };
                session.EnqueueMessageEncrypted(message);
            }

            session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterByName(playerName), character =>
            {
                if (character is null)
                {
                    sendNotFound();
                    return;
                }

                var targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                if (targetSession is null)
                {
                    sendNotFound();
                    return;
                }

                handler(targetSession.Player);
            }));
        }
    }
}
