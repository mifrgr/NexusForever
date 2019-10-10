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

                log.Info($"{session.Player.Name} has invited {request.PlayerName} to group");

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
                    log.Info($"Creating a new group");
                    group = GlobalGroupManager.CreateGroup(player);
                    group.IsRaid = true;
                }
                // Trying to invite without permission!
                else if (!player.GroupMember.CanInvite)
                {
                    sendGroupInviteResult(InviteResult.NotPermitted);
                    return;
                }

                log.Info($"Creating invite");
                group.CreateInvite(player.GroupMember, targetPlayer);
                
                sendGroupInviteResult(InviteResult.Sent, group.Id);

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
                targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                {
                    GroupId = group.Id,
                    GroupMembers = groupMembers
                });
            }));
        }
        
        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse request)
        {
            log.Info($"{session.Player.Name} has responded to group#{request.GroupId} invite with {request.Response}");

            var player = session.Player;

            var invite = player.GroupInvite;
            if (invite == null)
                return;

            var group = invite.Group;
            if (group.Id != request.GroupId)
                return;

            // Declined
            if (request.Response == InviteResponseResult.Declined)
            {
                group.DismissInvite(invite);

                Send(invite.Inviter, new ServerGroupInviteResult
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
            log.Info($"Add new member to the group");
            var newMember = group.AcceptInvite(invite);
            
            // Notify the inviter
            Send(invite.Inviter, new ServerGroupInviteResult
            {
                GroupId = request.GroupId,
                PlayerName = newMember.Player.Name,
                Result = InviteResult.Accepted
            });

            // if new group send ServerGroupJoin to all members
            if (isNewGroup)
            {
                Broadcast(group, member => BuildServerGroupJoin(group, member));
            }
            // otherwise send ServerGroupJoin to new member and ServerGroupMemberAdd to the rest
            else
            {
                Send(newMember, BuildServerGroupJoin(group, newMember));

                var addMember = BuildServerGroupMemberAdd(group, newMember);
                Broadcast(group, member => member.Id == newMember.Id ? null : addMember);
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
            log.Info($"{session.Player.Name} is leaving group#{request.GroupId} invite scope {request.Scope}");

            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            // party leader disbanded
            if (request.Scope == GroupLeaveScope.Disband)
            {
                if (!member.IsPartyLeader)
                    return;

                log.Info($"Disbanding the group#{group.Id}");
                Broadcast(group, BuildServerGroupLeave(group, RemoveReason.Disband));
                GlobalGroupManager.DisbandGroup(group);
                return;
            }

            // remove leaver
            var wasPartyLeader = member.IsPartyLeader;
            group.RemoveMember(member);
            Send(member, BuildServerGroupLeave(group, RemoveReason.Left));

            // No more group members? Disband
            if (group.IsEmpty)
            {
                log.Info($"Disbanding the group#{group.Id} because it is empty");
                Broadcast(group, BuildServerGroupLeave(group, RemoveReason.Disband));
                GlobalGroupManager.DisbandGroup(group);
                return;
            }

            // PartyLeader left?
            if (wasPartyLeader)
            {
                var nextLeader = group.NextPartyLeaderCandidate;
                group.SetPartyLeader(nextLeader);

                // send promotion
                Broadcast(group, BuildServerGroupPromote(group, nextLeader));
                
                // send flags
                Broadcast(group, BuildServerGroupMemberFlagsChanged(group, nextLeader, true));
            }

            // broadcast about that member left
            Broadcast(group, BuildServerGroupRemove(group, member, RemoveReason.Left));
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
            log.Info($"{session.Player.Name} in group#{request.GroupId} promote another member");
            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            // not a party leader?
            if (!member.IsPartyLeader)
                return;

            // next group lead
            var newLeader = group.FindMember(request.PlayerIdentity);
            if (newLeader == null)
                return;

            // promote
            log.Info($"Promoting {newLeader.Player.Name} in party leader");
            var oldLeader = group.PartyLeader;
            group.SetPartyLeader(newLeader);

            // notify the members
            Broadcast(group, BuildServerGroupPromote(group, newLeader));
            Broadcast(group, BuildServerGroupMemberFlagsChanged(group, oldLeader, true));
            Broadcast(group, BuildServerGroupMemberFlagsChanged(group, newLeader, true));
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
            log.Info($"{session.Player.Name} in group#{request.GroupId} changing member flags: {request.ChangedFlag}");
            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            // player whose flags will change
            var targetPlayer = group.FindMember(request.PlayerIdentity);
            if (targetPlayer == null)
                return;

            // not allowed?
            if (!member.CanUpdateFlags)
            {
                if (member.Id != targetPlayer.Id)
                    return;

                var allowedFlags = GroupMemberInfoFlags.RoleFlags
                                 | GroupMemberInfoFlags.HasSetReady
                                 | GroupMemberInfoFlags.Ready;
                if ((request.ChangedFlag & allowedFlags) != request.ChangedFlag)
                    return;
            }

            // update the player flags
            var value = (request.Flags & request.ChangedFlag) == request.ChangedFlag;
            targetPlayer.SetFlags(request.ChangedFlag, value);

            // send responses
            log.Info($"{targetPlayer.Player.Name} in group#{group.Id} flags changed to {targetPlayer.Flags}");
            Broadcast(group, BuildServerGroupMemberFlagsChanged(group, targetPlayer, false));
        }

        #endregion

        #region Ready Check

        [MessageHandler(GameMessageOpcode.ClientGroupSendReadyCheck)]
        public static void HandleGroupSendReadyCheck(WorldSession session, ClientGroupSendReadyCheck request)
        {
            log.Info($"{session.Player.Name} in group#{request.GroupId} initiates ready check");
            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            if (!member.CanReadyCheck)
                return;

            group.Members.ForEach(groupMember =>
            {
                groupMember.PrepareForReadyCheck();
                Broadcast(group, BuildServerGroupMemberFlagsChanged(group, groupMember, false));
            });

            var readyCheck = new ServerGroupSendReadyCheck
            {
                GroupId = group.Id,
                SenderIdentity = BuildTargetPlayerIdentity(member),
                Message = request.Message
            };
            Broadcast(group, readyCheck);
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
        /// Send message to the given group member
        /// </summary>
        private static void Send(GroupMember member, IWritable message)
        {
            member.Player.Session.EnqueueMessageEncrypted(message);
        }

        /// <summary>
        /// Broadcast message to all group members
        /// </summary>
        private static void Broadcast(Group group, IWritable message)
        {
            group.Members.ForEach(m => Send(m, message));
        }

        private delegate IWritable BroadcastCallback(GroupMember member);

        /// <summary>
        /// Broadcast generated message per member
        /// </summary>
        /// <param name="group">Group to broadcast to</param>
        /// <param name="callback">callback to generate message per every member</param>
        private static void Broadcast(Group group, BroadcastCallback callback)
        {
            group.Members.ForEach(member => {
                var value = callback(member);
                if (value != null)
                    Send(member, value);
            });
        }


        /// <summary>
        /// Validate that current player is in a group with given group ID
        /// </summary>
        /// <returns>Tuple containing Player, GroupMember and Group objects</returns>
        private static (GroupMember member, Group group) ValidateGroupMembership(WorldSession session, ulong groupId)
        {
            var member = session.Player.GroupMember;
            if (member == null)
                throw new InvalidPacketValueException();

            var group = member.Group;
            if (group == null || group.Id != groupId)
                throw new InvalidPacketValueException();

            return (member, group);
        }

        #endregion
    }
}
