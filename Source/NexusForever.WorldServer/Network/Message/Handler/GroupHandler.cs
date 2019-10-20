﻿using NexusForever.Shared.Game.Events;
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

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite request)
        {
            var player = session.Player;
            var group = player.GroupMember?.Group ?? GlobalGroupManager.CreateGroup(player);
            group.Invite(player, request.PlayerName);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupRequestJoin)]
        public static void HandleClientGroupRequestJoin(WorldSession session, ClientGroupRequestJoin request)
        {
            FindPlayer(session, request.PlayerName, targetPlayer =>
            {
                var targetMember = targetPlayer.GroupMember;
                if (targetMember is null)
                {
                    // should this be invite to group instead?
                    session.EnqueueMessageEncrypted(new ServerGroupRequestJoinResult {
                        GroupId = 0,
                        PlayerName = request.PlayerName,
                        Result = InviteResult.GroupNotFound,
                        IsJoin = true
                    });
                    return;
                }

                var group = targetMember.Group;
                group.RequestJoin(targetPlayer, session.Player);
            });
        }

        [MessageHandler(GameMessageOpcode.ClientGroupRequestJoinResponse)]
        public static void HandleClientGroupRequestJoinResponse(WorldSession session, ClientGroupRequestJoinResponse request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);

            FindPlayer(session, request.PlayerName, targetPlayer =>
            {
                var invite = targetPlayer.GroupInvite;
                if (invite == null)
                {
                    return;
                }
                
                group.HandleRequestJoin(invite, request.Accepted);
            });
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse request)
        {
            var player = session.Player;
            
            var invite = player.GroupInvite;
            if (invite == null)
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
            group.UpdateFlags(player, request.PlayerIdentity, request.ChangedFlags, request.Flags);
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
            group.UpdateFlags(player, request.Flag);
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
            if (member == null)
                throw new InvalidPacketValueException();

            var group = member.Group;
            if (group == null || group.Id != groupId)
                throw new InvalidPacketValueException();

            return (group, player);
        }
        
        /// <summary>
        /// Find player by name and get their session
        /// </summary>
        /// <param name="session">Requesting player's session</param>
        /// <param name="playerName">Player to find</param>
        /// <param name="handler">Callback to execute</param>
        private static void FindPlayer(WorldSession session, string playerName, Action<Player> handler)
        {
            void sendNotFound()
            {
                var message = new ServerGroupInviteResult
                {
                    GroupId = 0,
                    PlayerName = playerName,
                    Result = InviteResult.PlayerNotFound
                };
                session.EnqueueMessageEncrypted(message);
            }

            session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterByName(playerName), character =>
            {
                if (character == null)
                {
                    sendNotFound();
                    return;
                }

                var targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                if (targetSession == null)
                {
                    sendNotFound();
                    return;
                }

                handler(targetSession.Player);
            }));
        }
    }
}
