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

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite request)
        {
            session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterByName(request.PlayerName),
                character =>
            {
                if (character == null)
                {
                    session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                    {
                        GroupId = 0,
                        PlayerName = request.PlayerName,
                        Result = InviteResult.PlayerNotFound
                    });
                    return;
                }

                var group = GroupManager.CreateNewGroup();
                group.PartyLeaderCharacterId = session.Player.CharacterId;
                group.CreateNewInvite(character.Id);
                var member = group.CreateNewMember(session.Player.CharacterId);

                session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = group.GroupId,
                    PlayerName = request.PlayerName,
                    Result = InviteResult.Sent
                });

                WorldSession targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                var newMember = group.CreateNewMember(targetSession.Player.CharacterId);
                if (targetSession != null)
                {
                    targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                    {
                        GroupId = group.GroupId,
                        Unknown0 = 0,
                        Unknown1 = 0,
                        GroupMembers = new List<GroupMember>
                        {
                            new GroupMember
                            {
                                Name = targetSession.Player.Name,
                                Faction = targetSession.Player.Faction1,
                                Race = targetSession.Player.Race,
                                Class = targetSession.Player.Class,
                                Path = targetSession.Player.Path,
                                Level = (byte)targetSession.Player.Level,
                                GroupMemberId = (ushort)newMember.Id
                            }
                        }
                    });
                }
            }));
        }

        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse clientGroupInviteResponse)
        {
            log.Info($"{clientGroupInviteResponse.GroupId}, {clientGroupInviteResponse.Response}, {clientGroupInviteResponse.Unknown0}");

            var group = GroupManager.GetGroupById(clientGroupInviteResponse.GroupId);
            var invite = group.FindInvite(session.Player.CharacterId);

            if (group == null || invite == null)
                return;

            WorldSession targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == 1);

            // Declined
            if (clientGroupInviteResponse.Response == InviteResponseResult.Declined)
            {
                targetSession.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = clientGroupInviteResponse.GroupId,
                    PlayerName = session.Player.Name,
                    Result = InviteResult.Declined
                });

                group.DismissInvite(invite);
                if (group.IsEmpty)
                    GroupManager.DismissGroup(group);

                return;
            }

            // Accepted
            var member = group.AcceptInvite(invite);
            ServerGroupJoin join = new ServerGroupJoin
            {
                PlayerJoined = new TargetPlayerIdentity
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = 1
                },
                GroupId = group.GroupId,
                Unknown0 = 1,
                MaxSize = 5,
                LootRuleNormal = LootRule.RoundRobin,
                LootRuleThreshold = 2,
                LootThreshold = LootThreshold.Good,
                LootRuleHarvest = LootRuleHarvest.RoundRobin,
                GroupMembers = new List<ServerGroupJoin.GroupMemberInfo>
                {
                    new ServerGroupJoin.GroupMemberInfo
                    {
                        MemberIdentity = new TargetPlayerIdentity
                        {
                            RealmId = WorldServer.RealmId,
                            CharacterId = 1
                        },
                        Flags = 0,
                        GroupMember = new GroupMember
                        {
<<<<<<< refs/remotes/Party-Time/party-time
                            Name = targetSession.Player.Name,
                            Faction = targetSession.Player.Faction1,
                            Race = targetSession.Player.Race,
                            Class = targetSession.Player.Class,
                            Path = targetSession.Player.Path,
                            Level = (byte)targetSession.Player.Level,
                            EffectiveLevel = (byte)targetSession.Player.Level,
=======
                            Name = session.Player.Name,
                            Faction = session.Player.Faction1,
                            Race = session.Player.Race,
                            Class = session.Player.Class,
                            Path = session.Player.Path,
                            Level = (byte)session.Player.Level,
                            EffectiveLevel = 0,
>>>>>>> Changes
                            GroupMemberId = (ushort)member.Id,
                            UnknownStruct0List = new List<GroupMember.UnknownStruct0>
                            {
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 96
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 96
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 96
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 96
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 96
                                }
                            },
                            Unknown8 = 0,
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
<<<<<<< refs/remotes/Party-Time/party-time
                            WorldZoneId = (ushort)targetSession.Player.Zone.Id,
                            Unknown25 = 2725,
=======
                            WorldZoneId = (ushort)session.Player.Zone.Id,
                            Unknown25 = 1873,
>>>>>>> Idk
                            Unknown26 = 1,
                            Unknown27 = true,
                            Unknown28 = 0,
                            Unknown29 = 0,
                            UnknownStruct1List = new List<GroupMember.UnknownStruct1>()
                        },
                        GroupIndex = 1
                    },
                    new ServerGroupJoin.GroupMemberInfo
                    {
                        MemberIdentity = new TargetPlayerIdentity
                        {
                            RealmId = WorldServer.RealmId,
                            CharacterId = 2
                        },
                        Flags = 0,
                        GroupMember = new GroupMember
                        {
<<<<<<< refs/remotes/Party-Time/party-time
                            Name = session.Player.Name,
                            Faction = session.Player.Faction1,
                            Race = session.Player.Race,
                            Class = session.Player.Class,
                            Path = session.Player.Path,
                            Level = (byte)session.Player.Level,
                            EffectiveLevel = (byte)session.Player.Level,
=======
                            Name = targetSession.Player.Name,
                            Faction = targetSession.Player.Faction1,
                            Race = targetSession.Player.Race,
                            Class = targetSession.Player.Class,
                            Path = targetSession.Player.Path,
                            Level = (byte)targetSession.Player.Level,
                            EffectiveLevel = 0,
>>>>>>> Changes
                            GroupMemberId = (ushort)group.PartyLeader.Id,
                            UnknownStruct0List = new List<GroupMember.UnknownStruct0>
                            {
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 48
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 48
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 48
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 96
                                },
                                new GroupMember.UnknownStruct0
                                {
                                    Unknown6 = 0,
                                    Unknown7 = 96
                                }
                            },
                            Unknown8 = 0,
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
                            WorldZoneId = (ushort)session.Player.Zone.Id,
                            Unknown25 = 2725,
                            Unknown26 = 1,
                            Unknown27 = true,
                            Unknown28 = 0,
                            Unknown29 = 0,
                            UnknownStruct1List = new List<GroupMember.UnknownStruct1>()
                        },
                        GroupIndex = 2
                    },
                },
                LeaderIdentity = new TargetPlayerIdentity
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = 1
                },
                Realm = WorldServer.RealmId
            };

            session.EnqueueMessageEncrypted(join);

            targetSession.EnqueueMessageEncrypted(new ServerGroupInviteResult
            {
                GroupId = group.GroupId,
                PlayerName = session.Player.Name,
                Result = InviteResult.Accepted
            });

            targetSession.EnqueueMessageEncrypted(join);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            log.Info($"{request.GroupId} {request.Unk1}");
            var group = GroupManager.GetGroupById(request.GroupId);
            if (group == null)
                return;

            WorldSession targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == group.PartyLeaderCharacterId);

            group.RemoveMember(group.FindMemberByCharacterId(session.Player.CharacterId));
            session.EnqueueMessageEncrypted(new ServerGroupLeave
            {
                GroupId = group.GroupId,
                MemberId = (uint)group.FindMemberByCharacterId(session.Player.CharacterId).Id,
                UnkIdentity = new TargetPlayerIdentity()
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = session.Player.Guid
                },
                RemoveReason = RemoveReason.Left
            });

            targetSession.EnqueueMessageEncrypted(new ServerGroupLeave
            {
                GroupId = group.GroupId,
                MemberId = (uint)group.FindMemberByCharacterId(session.Player.CharacterId).Id,
                UnkIdentity = new TargetPlayerIdentity()
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = session.Player.Guid
                },
                RemoveReason = RemoveReason.Left
            });

            // If there is going to only be one player the group should be auto disbaned
            // This does not actually do anything.
            if (group.Members.Count - 1 < 2)
            {
                foreach (Group.Member member in group.Members)
                    group.RemoveMember(member);

                session.EnqueueMessageEncrypted(new ServerGroupLeave
                {
                    GroupId = group.GroupId,
                    MemberId = 1,
                    UnkIdentity = new TargetPlayerIdentity()
                    {
                        RealmId = WorldServer.RealmId,
                        CharacterId = session.Player.Guid
                    },
                    RemoveReason = RemoveReason.Disband
                });

                targetSession.EnqueueMessageEncrypted(new ServerGroupLeave
                {
                    GroupId = group.GroupId,
                    MemberId = 1,
                    UnkIdentity = new TargetPlayerIdentity()
                    {
                        RealmId = WorldServer.RealmId,
                        CharacterId = session.Player.Guid
                    },
                    RemoveReason = RemoveReason.Disband
                });
            }
        }
    }
}
