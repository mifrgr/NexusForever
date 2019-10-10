using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity;
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
                        PlayerName = request.PlayerName,
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

                var player = session.Player;
                var targetPlayer = targetSession.Player;

                // Player already in the group?
                if (targetPlayer.GroupMember != null)
                {
                    sendGroupInviteResult(InviteResult.PlayerAlreadyInGroup);
                    return;
                }

                // Player already has a pending invite
                if (targetPlayer.GroupInvite != null)
                {
                    sendGroupInviteResult(InviteResult.PlayerAlreadyInvited);
                    return;
                }

                // Inviting yourself?
                if (player.Guid == targetPlayer.Guid)
                {
                    sendGroupInviteResult(InviteResult.CannotInviteYourself);
                    return;
                }

                // are we creating a new group, or inviting into
                // existing one?
                var group = player.GroupMember?.Group;
                if (group == null)
                {
                    group = GlobalGroupManager.CreateGroup(player);
                    group.IsInstance = true;
                }
                // Not party leader trying to invite? Sneaky!
                else if (!player.GroupMember.CanInvite)
                {
                    sendGroupInviteResult(InviteResult.NotPermitted);
                    return;
                }

                // Create invite and send notifications
                group.CreateInvite(player.GroupMember, targetPlayer);

                var groupMembers = new List<Member>();
                foreach (var member in group.Members)
                {
                    groupMembers.Add(new Member
                    {
                        Name = member.Player.Name,
                        Faction = member.Player.Faction1,
                        Race = member.Player.Race,
                        Class = member.Player.Class,
                        Sex = member.Player.Sex,
                        Path = member.Player.Path,
                        Level = (byte)member.Player.Level,
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

            var player = session.Player;

            var invite = player.GroupInvite;
            if (invite == null)
                return;

            var group = invite.Group;
            if (group.Id != clientGroupInviteResponse.GroupId)
                return;

            // Declined
            if (clientGroupInviteResponse.Response == InviteResponseResult.Declined)
            {
                group.DismissInvite(invite);

                invite.Inviter.Player.Session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                {
                    GroupId = group.Id,
                    PlayerName = invite.Player.Name,
                    Result = InviteResult.Declined
                });

                // Remove Group and Members
                if (group.IsEmpty)
                    GlobalGroupManager.DisbandGroup(group);

                return;
            }

            var isNewGroup = group.IsEmpty;

            // Accepted. Add member and broadcast the info
            var newMember = group.AcceptInvite(invite);
            
            // Notify the inviter
            invite.Inviter.Player.Session.EnqueueMessageEncrypted(new ServerGroupInviteResult
            {
                GroupId = clientGroupInviteResponse.GroupId,
                PlayerName = newMember.Player.Name,
                Result = InviteResult.Accepted
            });

            // if new group send ServerGroupJoin to all members
            if (isNewGroup)
            {
                foreach (var member in group.Members)
                {
                    var groupJoin = BuildServerGroupJoin(group, member);
                    member.Player.Session.EnqueueMessageEncrypted(groupJoin);
                }
            }
            // otherwise send ServerGroupJoin to new member and ServerGroupMemberAdd to the rest
            else
            {
                var groupJoin = BuildServerGroupJoin(group, newMember);
                newMember.Player.Session.EnqueueMessageEncrypted(groupJoin);

                var addMember = BuildServerGroupMemberAdd(group, newMember);
                foreach (var member in group.Members)
                {
                    if (member.Id == newMember.Id)
                        continue;

                    member.Player.Session.EnqueueMessageEncrypted(addMember);
                }
            }
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupJoin.GroupMemberInfo BuildGroupMemberInfo(GroupMember member, uint groupIndex)
        {
            return new ServerGroupJoin.GroupMemberInfo
            {
                MemberIdentity = BuildTargetPlayerIdentity(member),
                Flags = member.Flags,
                GroupMember = new Member
                {
                    Name = member.Player.Name,
                    Faction = member.Player.Faction1,
                    Race = member.Player.Race,
                    Class = member.Player.Class,
                    Sex = member.Player.Sex,
                    Level = (byte)member.Player.Level,
                    EffectiveLevel = (byte)member.Player.Level,
                    Path = member.Player.Path,
                    GroupMemberId = member.Id,
                    Unknown8 = 1, // Something to do with Mentoring, Sets mentoring of first player that isn't you
                    Unknown9 = 1, // This and Unknown8 have to both be 1
                    Unknown10 = 1,
                    Realm = WorldServer.RealmId,
                    WorldZoneId = (ushort)member.Player.Zone.Id,
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
                groupMembers.Add(BuildGroupMemberInfo(groupMember, groupIndex++));
            }

            return new ServerGroupJoin
            {
                JoinedPlayer = BuildTargetPlayerIdentity(member),
                GroupId = group.Id,
                GroupType = group.Flags,
                MaxSize = group.MaxSize,
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
            var memberInfo = BuildGroupMemberInfo(member, groupIndex);
            return new ServerGroupMemberAdd
            {
                GroupId = group.Id,
                AddMemberInfo = memberInfo
            };
        }

        #endregion

        #region Disband / Leave group

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            var (player, member, group) = ValidateGroupMembership(session, request.GroupId);

            // party leader disbanded
            if (request.Scope == GroupLeaveScope.Disband)
            {
                if (!member.IsPartyLeader)
                    return;

                var groupDisband = BuildServerGroupLeave(group, RemoveReason.Disband);
                group.Members.ForEach(m => m.Player.Session.EnqueueMessageEncrypted(groupDisband));

                GlobalGroupManager.DisbandGroup(group);
                return;
            }

            // remove leaver
            var wasPartyLeader = member.IsPartyLeader;
            group.RemoveMember(member);
            var groupLeave = BuildServerGroupLeave(group, RemoveReason.Left);
            member.Player.Session.EnqueueMessageEncrypted(groupLeave);

            // No more group members? Disband
            if (group.IsEmpty)
            {
                var groupDisband = BuildServerGroupLeave(group, RemoveReason.Disband);
                group.Members.ForEach(m => m.Player.Session.EnqueueMessageEncrypted(groupDisband));
                GlobalGroupManager.DisbandGroup(group);
                return;
            }

            // PartyLeader left?
            if (wasPartyLeader)
            {
                var nextLeader = group.NextPartyLeaderCandidate;
                group.SetPartyLeader(nextLeader);

                // send promotion
                var promotion = BuildServerGroupPromote(group, nextLeader);
                group.Members.ForEach(m => m.Player.Session.EnqueueMessageEncrypted(promotion));
                
                // send flags
                foreach (var groupMember in group.Members)
                {
                    var memberFlags = BuildServerGroupMemberFlagsChanged(group, groupMember, true);
                    nextLeader.Player.Session.EnqueueMessageEncrypted(memberFlags);
                }
            }

            // broadcast about that member left
            var groupRemove = BuildServerGroupRemove(group, member, RemoveReason.Left);
            group.Members.ForEach(m => m.Player.Session.EnqueueMessageEncrypted(groupRemove));
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupLeave BuildServerGroupLeave(Group group, RemoveReason reason)
        {
            return new ServerGroupLeave
            {
                GroupId = group.Id,
                Reason = reason
            };
        }

        /// TODO: Refactor to a proper place
        private static ServerGroupRemove BuildServerGroupRemove(Group group, GroupMember leavingMember, RemoveReason reason)
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
            var (player, member, group) = ValidateGroupMembership(session, request.GroupId);

            // not a party leader?
            if (!member.IsPartyLeader)
                return;

            // next group lead
            var newLeader = group.Members.Find(m => m.Player.CharacterId == request.PlayerIdentity.CharacterId);
            if (newLeader == null)
                return;

            // promote
            var groupPromote = BuildServerGroupPromote(group, newLeader);
            group.Members.ForEach(m => m.Player.Session.EnqueueMessageEncrypted(groupPromote));

            // update flags
            var oldLeader = group.PartyLeader;
            group.SetPartyLeader(newLeader);

            var oldLeaderFlags = BuildServerGroupMemberFlagsChanged(group, oldLeader, true);
            oldLeader.Player.Session.EnqueueMessageEncrypted(oldLeaderFlags);

            foreach (var groupMember in group.Members)
            {
                var memberFlags = BuildServerGroupMemberFlagsChanged(group, groupMember, true);
                newLeader.Player.Session.EnqueueMessageEncrypted(memberFlags);
            }
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
            return new ServerGroupMemberFlagsChanged
            {
                GroupId         = group.Id,
                MemberId        = member.Id,
                PlayerIdentity  = BuildTargetPlayerIdentity(member),
                Flags           = member.Flags,
                FromPromotion   = fromPromotion
            };
        }

        #endregion

        #region Update Roles

        [MessageHandler(GameMessageOpcode.ClientGroupSetRole)]
        public static void HandleGroupSetRole(WorldSession session, ClientGroupSetRole request)
        {
            var (player, member, group) = ValidateGroupMembership(session, request.GroupId);

            // not allowed?
            if (!member.IsPartyLeader)
                return;

            // player whose flags will change
            var targetPlayer = group.Members.Find(m => m.Player.CharacterId == request.PlayerIdentity.CharacterId);
            if (targetPlayer == null)
                return;

            // update the player flags
            var value = (request.Flags & request.ChangedFlag) == request.ChangedFlag;
            targetPlayer.SetPermissonFlags(request.ChangedFlag, value);

            // send responses
            var updateFlags = BuildServerGroupMemberFlagsChanged(group, targetPlayer, false);
            targetPlayer.Player.Session.EnqueueMessageEncrypted(updateFlags);
            member.Player.Session.EnqueueMessageEncrypted(updateFlags);
        }

        #endregion

        #region Ready Check

        [MessageHandler(GameMessageOpcode.ClientGroupSendReadyCheck)]
        public static void HandleGroupSendReadyCheck(WorldSession session, ClientGroupSendReadyCheck request)
        {
            var (player, member, group) = ValidateGroupMembership(session, request.GroupId);

            var readyCheck = new ServerGroupSendReadyCheck
            {
                GroupId = group.Id,
                SenderIdentity = BuildTargetPlayerIdentity(member),
                Message = request.Message
            };
            group.Members.ForEach(m => m.Player.Session.EnqueueMessageEncrypted(readyCheck));
        }

        #endregion

        #region Helpers

        /// TODO: Refactor to a proper place
        private static TargetPlayerIdentity BuildTargetPlayerIdentity(GroupMember member)
        {
            return new TargetPlayerIdentity
            {
                RealmId = WorldServer.RealmId,
                CharacterId = member.Player.CharacterId
            };
        }

        /// <summary>
        /// Validate that current player is in a group with given group ID
        /// </summary>
        /// <returns>Tuple containing Player, GroupMember and Group objects</returns>
        private static (Player player, GroupMember member, Group group) ValidateGroupMembership(WorldSession session, ulong groupId)
        {
            var player = session.Player;
            
            var member = player.GroupMember;
            if (member == null)
                throw new InvalidPacketValueException();

            var group = member.Group;
            if (group == null || group.Id != groupId)
                throw new InvalidPacketValueException();

            return (player, member, group);
        }

        #endregion
    }
}
