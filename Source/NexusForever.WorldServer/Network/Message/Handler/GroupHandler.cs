using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Group;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite request)
        {
            session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterByName(request.PlayerName), character =>
            {
                var inviteResult = InviteResult.Sent;

                if (character == null)
                    inviteResult = InviteResult.PlayerNotFound;

                session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId     = 0,
                    PlayerName  = request.PlayerName,
                    Result      = inviteResult
                });

                if (inviteResult == InviteResult.Sent)
                {
                    var targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                    if (targetSession != null)
                    {
                        var group = GroupManager.CreateNewGroup(session.Player.Guid);
                        var member = group.CreateNewMember(session);
                        group.PartyLead = session.Player.CharacterId;

                        targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                        {
                            GroupId     = group.GroupId,
                            Unknown0    = 0,
                            Unknown1    = 0,
                            GroupMembers = new List<GroupMember>
                            {
                                new GroupMember
                                {
                                    Name            = session.Player.Name,
                                    Faction         = session.Player.Faction1,
                                    Race            = session.Player.Race,
                                    Class           = session.Player.Class,
                                    Path            = session.Player.Path,
                                    Level           = (byte)session.Player.Level,
                                    GroupMemberId   = (ushort)member.Id
                                }
                            }
                        });
                    }
                }
            }));
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession invited, ClientGroupInviteResponse clientGroupInviteResponse)
        {
            log.Info($"{clientGroupInviteResponse.GroupId}, {clientGroupInviteResponse.Response}, {clientGroupInviteResponse.Unknown0}");

            var group = GroupManager.GetGroupById(clientGroupInviteResponse.GroupId);
            if (group == null)
                return;

            WorldSession invitee = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == group.PartyLead);
            if (invitee == null)
                GroupManager.RemoveGroup(group);

            // Declined
            if (clientGroupInviteResponse.Response == InviteResponseResult.Declined)
            {
                invitee.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = clientGroupInviteResponse.GroupId,
                    PlayerName = invited.Player.Name,
                    Result = InviteResult.Declined
                });

                // Remove Group and Members
                if (group.Members.Count - 1 < 2)
                    GroupManager.RemoveGroup(group);

                return;
            }

            // Accepted
            var leader      = group.FindMemberByPlayerGuid(invitee.Player.Guid);
            var newMember   = group.CreateNewMember(invited);
            var joinGroup   = new ServerGroupJoin
            {
                PlayerJoined = new TargetPlayerIdentity
                {
                    RealmId         = WorldServer.RealmId,
                    CharacterId     = invited.Player.CharacterId
                },
                GroupId             = clientGroupInviteResponse.GroupId,
                Unknown0            = 1,
                MaxSize             = 5,
                LootRuleNormal      = LootRule.RoundRobin,
                LootRuleThreshold   = 2,
                LootThreshold       = LootThreshold.Good,
                LootRuleHarvest     = LootRuleHarvest.RoundRobin,
                GroupMembers        = new List<ServerGroupJoin.GroupMemberInfo>
                {
                    new ServerGroupJoin.GroupMemberInfo
                    {
                        MemberIdentity = new TargetPlayerIdentity
                        {
                            RealmId     = WorldServer.RealmId,
                            CharacterId = invitee.Player.CharacterId
                        },
                        Flags = 8198,
                        GroupMember = new GroupMember
                        {
                            Name            = invitee.Player.Name,
                            Faction         = invitee.Player.Faction1,
                            Race            = invitee.Player.Race,
                            Class           = invitee.Player.Class,
                            Path            = invitee.Player.Path,
                            Level           = (byte)invitee.Player.Level,
                            GroupMemberId   = (ushort)leader.Id,
                            Realm           = WorldServer.RealmId,
                            WorldZoneId     = (ushort)invitee.Player.Zone.Id,
                            Unknown25       = 2725,
                            Unknown26       = 1,
                            Unknown27       = true,
                        },
                        GroupIndex = 0
                    },
                    new ServerGroupJoin.GroupMemberInfo
                    {
                        MemberIdentity = new TargetPlayerIdentity
                        {
                            RealmId     = WorldServer.RealmId,
                            CharacterId = invited.Player.CharacterId
                        },
                        Flags = 8192,
                        GroupMember = new GroupMember
                        {
                            Name            = invited.Player.Name,
                            Faction         = invited.Player.Faction1,
                            Race            = invited.Player.Race,
                            Class           = invited.Player.Class,
                            Path            = invited.Player.Path,
                            Level           = (byte)invited.Player.Level,
                            GroupMemberId   = (ushort)newMember.Id,
                            Realm           = WorldServer.RealmId,
                            WorldZoneId     = (ushort)invited.Player.Zone.Id,
                            Unknown25       = 2725,
                            Unknown26       = 1,
                            Unknown27       = true,
                        },
                        GroupIndex = GroupManager.NextGroupIndex
                    },
                },
                LeaderIdentity = new TargetPlayerIdentity
                {
                    RealmId     = WorldServer.RealmId,
                    CharacterId = invitee.Player.CharacterId
                },
                Realm = WorldServer.RealmId
            };

            foreach (var member in group.Members)
            {
                if (member != newMember)
                {
                    member.PlayerSession.EnqueueMessageEncrypted(new ServerGroupInviteResult
                    {
                        GroupId = clientGroupInviteResponse.GroupId,
                        PlayerName = invited.Player.Name,
                        Result = InviteResult.Accepted
                    });
                }

                member.PlayerSession.EnqueueMessageEncrypted(joinGroup);
            }
        }

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            log.Info($"{request.GroupId} {request.Unk1}");
            var group = GroupManager.GetGroupById(request.GroupId);
            if (group == null)
                return;

            var members     = group.Members.ToList();
            var memberId    = group.FindMemberByPlayerGuid(session.Player.Guid).Id;

            // If there is going to only be one player the group should be auto disbaned
            // This does not actually do anything.
            if (group.Members.Count - 1 < 2)
            {
                var targetMember = group.FindMemberByPlayerGuid(session.Player.Guid);
                GroupManager.SendGroupRemove(session, targetMember.PlayerSession, group, memberId, RemoveReason.Disband);
                group.RemoveMember(group.FindMemberByPlayerGuid(session.Player.Guid));
                GroupManager.RemoveGroup(group);
            }
            else
                foreach (var member in members)
                    GroupManager.SendGroupRemove(session, member.PlayerSession, group, memberId, RemoveReason.Left);
        }
    }
}
