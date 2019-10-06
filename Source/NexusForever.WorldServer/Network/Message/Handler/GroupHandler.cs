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
                void sendGroupInviteResult(InviteResult result, ulong groupId = 0)
                {
                    session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                    {
                        GroupId = groupId,
                        PlayerName = character.Name,
                        Result = result
                    });
                }

                // Invalid character?
                if (character == null)
                {
                    sendGroupInviteResult(InviteResult.PlayerNotFound);
                    return;
                }

                // Player not online?
                var targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                if (targetSession == null)
                {
                    sendGroupInviteResult(InviteResult.PlayerNotFound);
                    return;
                }

                // Player already in the group?
                if (GroupManager.FindPlayerGroup(targetSession) != null)
                {
                    sendGroupInviteResult(InviteResult.PlayerAlreadyInGroup);
                    return;
                }

                // Player already has a pending invite
                if (GroupManager.FindPlayerInvite(targetSession) != null)
                {
                    sendGroupInviteResult(InviteResult.PlayerAlreadyInvited);
                    return;
                }

                // Inviting yourself?
                if (targetSession.Player.Guid == session.Player.Guid)
                {
                    sendGroupInviteResult(InviteResult.CannotInviteYourself);
                    return;
                }

                // are we creating a new group, or inviting into
                // existing one?
                var group = GroupManager.FindPlayerGroup(session);
                if (group == null) {
                    group = GroupManager.CreateGroup();
                    var leader = group.CreateMember(session);
                    group.PartyLeader = leader;
                }
                // Not party leader trying to invite? Sneaky!
                else if (group.PartyLeader.Guid != session.Player.Guid)
                {
                    sendGroupInviteResult(InviteResult.NotPermitted);
                    return;
                }

                // Create invite and send notifications

                var invite = group.CreateInvite(targetSession);
                invite.Inviter = group.FindMember(session);

                var groupMembers = new List<GroupMember>();
                foreach (var member in group.Members)
                {
                    groupMembers.Add(new GroupMember
                    {
                        Name = member.Session.Player.Name,
                        Faction = member.Session.Player.Faction1,
                        Race = member.Session.Player.Race,
                        Class = member.Session.Player.Class,
                        Sex = member.Session.Player.Sex,
                        Path = member.Session.Player.Path,
                        Level = (byte)member.Session.Player.Level,
                        GroupMemberId = (ushort)member.Id
                    });
                }

                sendGroupInviteResult(InviteResult.Sent, group.Id);

                targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                {
                    GroupId     = group.Id,
                    Unknown0    = 0,
                    Unknown1    = 0,
                    GroupMembers = groupMembers
                });
            }));
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse clientGroupInviteResponse)
        {
            log.Info($"{clientGroupInviteResponse.GroupId}, {clientGroupInviteResponse.Response}, {clientGroupInviteResponse.Unknown0}");

            var group = GroupManager.GetGroupById(clientGroupInviteResponse.GroupId);
            if (group == null)
                return;

            var invite = group.FindInvite(session);
            if (invite == null)
                return;

            // Declined
            if (clientGroupInviteResponse.Response == InviteResponseResult.Declined)
            {
                group.DismissInvite(invite);

                invite.Inviter.Session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = group.Id,
                    PlayerName = invite.Session.Player.Name,
                    Result = InviteResult.Declined
                });
                
                // Remove Group and Members
                if (group.IsEmpty)
                    GroupManager.DisbandGroup(group);

                return;
            }

            // Accepted. Add member and broadcast the info

            var newMember = group.AcceptInvite(invite);
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
                        CharacterId = member.Session.Player.CharacterId
                    },
                    Flags = 0,
                    GroupMember = new GroupMember
                    {
                        Name = member.Session.Player.Name,
                        Faction = member.Session.Player.Faction1,
                        Race = member.Session.Player.Race,
                        Class = member.Session.Player.Class,
                        Sex = member.Session.Player.Sex,
                        Level = (byte)member.Session.Player.Level,
                        EffectiveLevel = (byte)member.Session.Player.Level,
                        Path = member.Session.Player.Path,
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
                        Unknown9 = 1, // This and Unknown8 have to both be 1
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
                        WorldZoneId = (ushort)member.Session.Player.Zone.Id,
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
                    CharacterId     = newMember.Session.Player.CharacterId
                },
                GroupId             = group.Id,
                GroupType           = (uint)GroupType.Standard,
                MaxSize             = 5,
                LootRuleNormal      = LootRule.NeedBeforeGreed,         // Under LootThreshold rarity (For Raid)
                LootRuleThreshold   = LootRule.RoundRobin,              // This is the selection for Loot Rules in the UI / Over LootTreshold rarity (For Raid)
                LootThreshold       = LootThreshold.Excellent,
                LootRuleHarvest     = LootRuleHarvest.FirstTagger,      // IDK were it shows this setting in the UI
                GroupMembers        = groupMembersInfo,
                LeaderIdentity = new TargetPlayerIdentity
                {
                    RealmId     = WorldServer.RealmId,
                    CharacterId = group.PartyLeader.Session.Player.CharacterId
                },
                Realm = WorldServer.RealmId
            };

            foreach (var member in group.Members)
            {
                if (member != newMember && member.Guid == invite.Inviter.Session.Player.Guid)
                {
                    member.Session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                    {
                        GroupId = clientGroupInviteResponse.GroupId,
                        PlayerName = newMember.Session.Player.Name,
                        Result = InviteResult.Accepted
                    });
                }

                GroupMemberInfoFlags flags = member.Guid == group.PartyLeader.Guid
                                           ? GroupMemberInfoFlags.GroupAdmin
                                           : GroupMemberInfoFlags.GroupMember;
                foreach (var groupmember in joinGroup.GroupMembers)
                    groupmember.Flags = flags;

                member.Session.EnqueueMessageEncrypted(joinGroup);
            }
        }

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            var group = GroupManager.GetGroupById(request.GroupId);
            if (group == null)
                return;

            // TODO
            GroupManager.DisbandGroup(group);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSetRole)]
        public static void HandleGroupSetRole(WorldSession session, ClientGroupSetRole request)
        {
            log.Info($"GroupId: {request.GroupId} Unk1: {request.Unk1} Unk2: {request.Unk2}");
        }
    }
}
