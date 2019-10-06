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
                        var newMember = group.CreateNewMember(session);
                        group.PartyLead = session.Player.CharacterId;

                        var groupMembers = new List<GroupMember>();
                        foreach (var member in group.Members)
                        {
                            groupMembers.Add(new GroupMember
                            {
                                Name = member.PlayerSession.Player.Name,
                                Faction = member.PlayerSession.Player.Faction1,
                                Race = member.PlayerSession.Player.Race,
                                Class = member.PlayerSession.Player.Class,
                                Path = member.PlayerSession.Player.Path,
                                Level = (byte)member.PlayerSession.Player.Level,
                                GroupMemberId = (ushort)member.Id
                            });
                        }

                        targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                        {
                            GroupId     = group.GroupId,
                            Unknown0    = 0,
                            Unknown1    = 0,
                            GroupMembers = groupMembers
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
                if (group.Members.Count <= 1)
                    GroupManager.RemoveGroup(group);

                return;
            }

            // Accepted
            var leader           = group.FindMember(invitee);
            var newMember        = group.CreateNewMember(invited);
            var groupMembersInfo = new List<ServerGroupJoin.GroupMemberInfo>();

            // Create ServerGroupJoin.GroupMemberInfo for every member
            uint groupIndex = 1;
            foreach (var member in group.Members)
            {
                groupMembersInfo.Add(new ServerGroupJoin.GroupMemberInfo
                {
                    MemberIdentity = new TargetPlayerIdentity
                    {
                        RealmId = WorldServer.RealmId,
                        CharacterId = member.PlayerSession.Player.CharacterId
                    },
                    Flags = 8192,
                    GroupMember = new GroupMember
                    {
                        Name = member.PlayerSession.Player.Name,
                        Faction = member.PlayerSession.Player.Faction1,
                        Race = member.PlayerSession.Player.Race,
                        Class = member.PlayerSession.Player.Class,
                        Unknown2 = 0,
                        Level = (byte)member.PlayerSession.Player.Level,
                        EffectiveLevel = (byte)member.PlayerSession.Player.Level,
                        Path = member.PlayerSession.Player.Path,
                        Unknown4 = 0,
                        GroupMemberId = (ushort)member.Id,
                        UnknownStruct0List = new List<GroupMember.UnknownStruct0>
                        {
                            new GroupMember.UnknownStruct0()
                            {
                                Unknown6 = 0,
                                Unknown7 = 48
                            },
                            new GroupMember.UnknownStruct0()
                            {
                                Unknown6 = 0,
                                Unknown7 = 48
                            },
                            new GroupMember.UnknownStruct0()
                            {
                                Unknown6 = 0,
                                Unknown7 = 48
                            },
                            new GroupMember.UnknownStruct0()
                            {
                                Unknown6 = 0,
                                Unknown7 = 48
                            },
                            new GroupMember.UnknownStruct0()
                            {
                                Unknown6 = 0,
                                Unknown7 = 48
                            }
                        },
                        Unknown8 = 1, // Something to do with Mentoring, Sets mentoring of first player that isn't you
                        Unknown9 = 0,
                        Unknown10 = 0,
                        Unknown11 = 0,
                        Unknown12 = 0,
                        Unknown13 = 0,
                        Unknown14 = 0,
                        Unknown15 = 0,
                        Unknown16 = 0,
                        Unknown17 = 0,
                        Unknown18 = 0,
                        Unknown19 = 0,
                        Unknown20 = 0,
                        Unknown21 = 0,
                        Unknown22 = 0,
                        Realm = WorldServer.RealmId,
                        WorldZoneId = (ushort)member.PlayerSession.Player.Zone.Id,
                        Unknown25 = 1873,
                        Unknown26 = 1,
                        SyncedToGroup = true,
                        Unknown28 = 0,
                        Unknown29 = 0
                    },
                    GroupIndex = groupIndex++
                });
            }

            var joinGroup = new ServerGroupJoin
            {
                PlayerJoined = new TargetPlayerIdentity
                {
                    RealmId         = WorldServer.RealmId,
                    CharacterId     = newMember.PlayerSession.Player.CharacterId
                },
                GroupId             = group.GroupId,
                GroupType           = (uint)GroupType.Standard,
                MaxSize             = 5,
                LootRuleNormal      = LootRule.NeedBeforeGreed, // Under LootThreshold rarity (For Raid)
                LootRuleThreshold   = LootRule.RoundRobin, // This is the selection for Loot Rules in the UI / Over LootTreshold rarity (For Raid)
                LootThreshold       = LootThreshold.Excellent,
                LootRuleHarvest     = LootRuleHarvest.FirstTagger, // IDK were it shows this setting in the UI
                GroupMembers        = groupMembersInfo,
                LeaderIdentity = new TargetPlayerIdentity
                {
                    RealmId     = WorldServer.RealmId,
                    CharacterId = leader.PlayerSession.Player.CharacterId
                },
                Realm = WorldServer.RealmId
            };

            foreach (var member in group.Members)
            {
                if (member != newMember && member.Guid == invitee.Player.Guid)
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

            var members  = group.Members.ToList();
            var memberId = group.FindMember(session).Id;

            if (group.Members.Count - 1 < 2)
            {
                var targetMember = group.FindMember(session);
                group.RemoveMember(group.FindMember(session));

                GroupManager.SendGroupRemove(session, targetMember.PlayerSession, group, memberId, RemoveReason.Disband);
                GroupManager.RemoveGroup(group);
            }
            else
                foreach (var member in members)
                    GroupManager.SendGroupRemove(session, member.PlayerSession, group, memberId, RemoveReason.Left);
        }
    }
}
