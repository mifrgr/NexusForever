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

        #region Invite to / join group

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
                if (group == null)
                {
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

                var groupMembers = new List<Member>();
                foreach (var member in group.Members)
                {
                    groupMembers.Add(new Member
                    {
                        Name = member.Session.Player.Name,
                        Faction = member.Session.Player.Faction1,
                        Race = member.Session.Player.Race,
                        Class = member.Session.Player.Class,
                        Sex = member.Session.Player.Sex,
                        Path = member.Session.Player.Path,
                        Level = (byte)member.Session.Player.Level,
                        GroupMemberId = member.Id
                    });
                }

                sendGroupInviteResult(InviteResult.Sent, group.Id);

                targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                {
                    GroupId = group.Id,
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

            var isNewGroup = group.IsEmpty;

            // Accepted. Add member and broadcast the info
            var newMember = group.AcceptInvite(invite);
            
            // Notify the inviter
            invite.Inviter.Session.EnqueueMessageEncrypted(new ServerGroupInviteResult
            {
                GroupId = clientGroupInviteResponse.GroupId,
                PlayerName = newMember.Session.Player.Name,
                Result = InviteResult.Accepted
            });

            // if new group send ServerGroupJoin to all members
            if (isNewGroup)
            {
                foreach (var member in group.Members)
                {
                    var groupJoin = BuildServerGroupJoin(group, member);
                    member.Session.EnqueueMessageEncrypted(groupJoin);
                }
            }
            // otherwise send ServerGroupJoin to new member and ServerGroupMemberAdd to the rest
            else
            {
                var groupJoin = BuildServerGroupJoin(group, newMember);
                newMember.Session.EnqueueMessageEncrypted(groupJoin);

                var addMember = BuildServerGroupMemberAdd(group, newMember);
                foreach (var member in group.Members)
                {
                    if (member.Guid == newMember.Guid)
                        continue;
                    member.Session.EnqueueMessageEncrypted(addMember);
                }
            }
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupJoin.GroupMemberInfo BuildGroupMemberInfo(Game.Group.Static.GroupMember member, GroupMemberInfoFlags flags, uint groupIndex)
        {
            return new ServerGroupJoin.GroupMemberInfo
            {
                MemberIdentity = BuildTargetPlayerIdentity(member),
                Flags = flags,
                GroupMember = new Member
                {
                    Name = member.Session.Player.Name,
                    Faction = member.Session.Player.Faction1,
                    Race = member.Session.Player.Race,
                    Class = member.Session.Player.Class,
                    Sex = member.Session.Player.Sex,
                    Level = (byte)member.Session.Player.Level,
                    EffectiveLevel = (byte)member.Session.Player.Level,
                    Path = member.Session.Player.Path,
                    GroupMemberId = member.Id,
                    Unknown8 = 1, // Something to do with Mentoring, Sets mentoring of first player that isn't you
                    Unknown9 = 1, // This and Unknown8 have to both be 1
                    Unknown10 = 1,
                    Realm = WorldServer.RealmId,
                    WorldZoneId = (ushort)member.Session.Player.Zone.Id,
                    Unknown25 = 1873,
                    Unknown26 = 1,
                    SyncedToGroup = true
                },
                GroupIndex = groupIndex
            };
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupJoin BuildServerGroupJoin(Group group, GroupMember member)
        {
            uint groupIndex = 1;
            var groupMembers = new List<ServerGroupJoin.GroupMemberInfo>();
            foreach (var groupMember in group.Members)
            {
                var flags = groupMember.Guid == group.PartyLeader.Guid
                          ? GroupMemberInfoFlags.GroupAdmin
                          : GroupMemberInfoFlags.GroupMember;
                groupMembers.Add(BuildGroupMemberInfo(groupMember, flags, groupIndex++));
            }

            return new ServerGroupJoin
            {
                JoinedPlayer = BuildTargetPlayerIdentity(member),
                GroupId = group.Id,
                GroupType = GroupTypeFlags.OpenWorld,
                MaxSize = 5,
                LootRuleNormal = LootRule.NeedBeforeGreed,         // Under LootThreshold rarity (For Raid)
                LootRuleThreshold = LootRule.RoundRobin,              // This is the selection for Loot Rules in the UI / Over LootTreshold rarity (For Raid)
                LootThreshold = LootThreshold.Excellent,
                LootRuleHarvest = LootRuleHarvest.FirstTagger,      // IDK were it shows this setting in the UI
                GroupMembers = groupMembers,
                LeaderIdentity = BuildTargetPlayerIdentity(group.PartyLeader),
                Realm = WorldServer.RealmId
            };
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupMemberAdd BuildServerGroupMemberAdd(Group group, GroupMember member)
        {
            var groupIndex = (uint)group.Members.IndexOf(member) + 1;
            var flags = member.Guid == group.PartyLeader.Guid
                      ? GroupMemberInfoFlags.GroupAdmin
                      : GroupMemberInfoFlags.GroupMember;
            var memberInfo = BuildGroupMemberInfo(member, flags, groupIndex);
            return new ServerGroupMemberAdd
            {
                GroupId = group.Id,
                AddMemberInfo = memberInfo
            };
        }

        #endregion

        #region Leave group

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            var group = GroupManager.GetGroupById(request.GroupId);
            if (group == null)
                return;

            var leavingMember = group.FindMember(session);
            if (leavingMember == null)
                return;

            // party leader disbanded
            if (request.Scope == GroupLeaveScope.Disband)
            {
                if (leavingMember.Guid != group.PartyLeader.Guid)
                    return;

                var groupDisband = buildServerGroupLeave(group, RemoveReason.Disband);
                foreach (var member in group.Members)
                {
                    member.Session.EnqueueMessageEncrypted(groupDisband);
                }

                GroupManager.DisbandGroup(group);
                return;
            }

            // member leaving with only party leader remaining?
            if (group.Members.Count == 2)
            {
                var groupLeave = buildServerGroupLeave(group, RemoveReason.Left);
                leavingMember.Session.EnqueueMessageEncrypted(groupLeave);
                group.RemoveMember(leavingMember);

                var groupDisband = buildServerGroupLeave(group, RemoveReason.Disband);
                group.Members[0].Session.EnqueueMessageEncrypted(groupDisband);

                GroupManager.DisbandGroup(group);
                return;
            }

            // is member who leaves a PartyLeader?
            if (leavingMember.Guid == group.PartyLeader.Guid)
            {
                // TODO pass party lead to next person in group
                return;
            }

            var leavingGroup = buildServerGroupLeave(group, RemoveReason.Left);
            leavingMember.Session.EnqueueMessageEncrypted(leavingGroup);
            group.RemoveMember(leavingMember);

            var groupRemove = buildServerGroupRemove(group, leavingMember, RemoveReason.Left);
            foreach (var member in group.Members)
            {
                member.Session.EnqueueMessageEncrypted(groupRemove);
            }
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupLeave buildServerGroupLeave(Group group, RemoveReason reason)
        {
            return new ServerGroupLeave
            {
                GroupId = group.Id,
                Reason = reason
            };
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupRemove buildServerGroupRemove(Group group, GroupMember leavingMember, RemoveReason reason)
        {
            return new ServerGroupRemove
            {
                GroupId = group.Id,
                MemberId = leavingMember.Id,
                PlayerLeave = BuildTargetPlayerIdentity(leavingMember),
                RemoveReason = reason
            };
        }

        #endregion

        #region Promotion to Party Lead

        [MessageHandler(GameMessageOpcode.ClientGroupPromote)]
        public static void HandleGroupPromote(WorldSession session, ClientGroupPromote request)
        {
            var group = GroupManager.FindPlayerGroup(session);
            if (group == null)
                return;

            var promoter = group.FindMember(session);
            if (promoter == null)
                return;

            // not a party leader?
            if (promoter.Guid != group.PartyLeader.Guid)
                return;

            var targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == request.PlayerIdentity.CharacterId);
            if (targetSession == null)
                return;

            var promotedMember = group.FindMember(targetSession);
            if (promotedMember == null)
                return;

            // promote
            var groupPromote = BuildServerGroupPromote(group, promotedMember);
            foreach (var member in group.Members)
            {
                member.Session.EnqueueMessageEncrypted(groupPromote);
            }

            // update flags
            var oldLeader = group.PartyLeader;
            group.PartyLeader = promotedMember;

            var oldLeaderFlags = BuildServerGroupMemberFlagsChanged(group, oldLeader, true);
            oldLeader.Session.EnqueueMessageEncrypted(oldLeaderFlags);

            var newLeaderFlags = BuildServerGroupMemberFlagsChanged(group, promotedMember, true);
            promotedMember.Session.EnqueueMessageEncrypted(newLeaderFlags);
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupPromote BuildServerGroupPromote(Group group, GroupMember member)
        {
            return new ServerGroupPromote
            {
                GroupId = group.Id,
                MemberId = member.Id,
                PlayerIdentity = BuildTargetPlayerIdentity(member)
            };
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupMemberFlagsChanged BuildServerGroupMemberFlagsChanged(Group group, GroupMember member, bool fromPromotion)
        {
            var flags = member.Guid == group.PartyLeader.Guid
                      ? GroupMemberInfoFlags.GroupAdmin
                      : GroupMemberInfoFlags.GroupMember;
            return new ServerGroupMemberFlagsChanged
            {
                GroupId         = group.Id,
                MemberId        = member.Id,
                PlayerIdentity  = BuildTargetPlayerIdentity(member),
                Flags           = flags,
                FromPromotion   = fromPromotion
            };
        }

        #endregion

        [MessageHandler(GameMessageOpcode.ClientGroupSetRole)]
        public static void HandleGroupSetRole(WorldSession session, ClientGroupSetRole request)
        {
            log.Info($"GroupId: {request.GroupId} Unk1: {request.Unk1} Unk2: {request.Unk2}");
        }

        #region Helpers

        /// TODO: Refactor to a proper place
        private static TargetPlayerIdentity BuildTargetPlayerIdentity(GroupMember member)
        {
            return new TargetPlayerIdentity
            {
                RealmId = WorldServer.RealmId,
                CharacterId = member.Session.Player.CharacterId
            };
        }

        #endregion
    }
}
