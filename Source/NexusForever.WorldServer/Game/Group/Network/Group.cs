using NexusForever.Shared;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Extensions;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    public partial class Group
    {
        #region Broadcast

        /// <summary>
        /// Generate message to broadcast
        /// </summary>
        /// <param name="member">message subject</param>
        /// <returns>message to send or null to skip</returns>
        public delegate IWritable? BroadcastCallback(GroupMember member);

        /// <summary>
        /// Broadcast message generated in callback to all members
        /// </summary>
        /// <param name="callback">callback to generate message per member</param>
        public void Broadcast(BroadcastCallback callback)
        {
            foreach (var member in GetMembers())
                if (callback(member) is IWritable message)
                    member.Send(message);
        }

        /// <summary>
        /// Broadcast given message to the whole group
        /// </summary>
        /// <param name="message">message to broadcast</param>
        /// <param name="excluded">do not send message to excluded member</param>
        public void Broadcast(IWritable message, GroupMember? excluded = null)
        {
            using(membersLock.GetReadLock())
            {
                foreach (var member in members)
                    if (member.Id != excluded?.Id)
                        member.Send(message);
            }
        }

        #endregion

        /// <summary>
        /// Send group <-> entity association to all group members who can see each other
        /// </summary>
        /// <param name="associate">bind entities to this group or unbind</param>
        /// <param name="member">If not null then only related to this member</param>
        public void SendGroupEntityAssociations(bool associate, GroupMember? member = null)
        {
            using(membersLock.GetReadLock())
            {
                // multicast to all
                if (member is null)
                {
                    var count = members.Count;

                    // pre-generate messages for every member
                    var messages = new ServerEntityGroupAssociation[count];
                    for (var i = 0; i < count; i++)
                        messages[i] = members[i].BuildServerEntityGroupAssociation(associate);

                    // send associations to all members about all other members
                    for (var t = 0; t < count; t++)
                    {
                        var target = members[t];
                        target.Send(messages[t]);

                        for (var s = t + 1; s < count; s++)
                        {
                            var subject = members[s];
                            if (target.Player.CanSeePlayer(subject.Player))
                            {
                                subject.Send(messages[t]);
                                target.Send(messages[s]);
                            }
                        }
                    }           
                    return;
                }

                // only specific member
                var message = member.BuildServerEntityGroupAssociation(associate);
                member.Send(message);

                foreach (var target in members)
                { 
                    if (target.Id != member.Id && target.Player.CanSeePlayer(member.Player))
                    {
                        target.Send(message);
                        member.Send(target.BuildServerEntityGroupAssociation(associate));
                    }
                }
            }
        }

        #region Packets

        /// <summary>
        /// Build Group Join packet for the given member
        /// </summary>
        /// <param name="member">new member who joined</param>
        public ServerGroupJoin BuildServerGroupJoin(GroupMember member)
        {
            uint groupIndex = 1;
            var groupMembers = new List<ServerGroupJoin.GroupMemberInfo>();
            using(membersLock.GetReadLock())
            { 
                foreach (var groupMember in members)
                    groupMembers.Add(groupMember.BuildGroupMemberInfo(groupIndex++));
            }

            return new ServerGroupJoin
            {
                JoinedPlayer = member.Player.BuildTargetPlayerIdentity(),
                GroupId = Id,
                GroupFlags = Flags,
                MaxSize = MaxSize,
                LootRuleNormal = LootRule.NeedBeforeGreed,          // Under LootThreshold rarity (For Raid)
                LootRuleThreshold = LootRule.RoundRobin,            // This is the selection for Loot Rules in the UI / Over LootTreshold rarity (For Raid)
                LootThreshold = LootThreshold.Excellent,
                LootRuleHarvest = LootRuleHarvest.FirstTagger,      // IDK were it shows this setting in the UI
                GroupMembers = groupMembers,
                LeaderIdentity = PartyLeader.Player.BuildTargetPlayerIdentity(),
                Realm = WorldServer.RealmId
            };
        }

        /// <summary>
        /// Build group join request packet
        /// </summary>
        public ServerGroupRequestJoin BuildServerGroupRequestJoin(GroupInvite invite)
        {
            return new ServerGroupRequestJoin
            {
                GroupId = Id,
                MemberInfo = invite.BuildGroupMemberInfo()
            };
        }

        /// <summary>
        /// Build Member Add packet for the given member
        /// </summary>
        public ServerGroupMemberAdd BuildServerGroupMemberAdd(GroupMember member)
        {
            using (membersLock.GetReadLock())
            {
                var groupIndex = (uint)members.IndexOf(member) + 1;
                var memberInfo = member.BuildGroupMemberInfo(groupIndex);
                return new ServerGroupMemberAdd
                {
                    GroupId = Id,
                    AddMemberInfo = memberInfo
                };
            }
        }

        /// <summary>
        /// Build Group Leave packet with the given reason
        /// </summary>
        public ServerGroupLeave BuildServerGroupLeave(RemoveReason reason)
        {
            return new ServerGroupLeave
            {
                GroupId = Id,
                Reason = reason
            };
        }

        /// <summary>
        /// Build invite result
        /// </summary>
        public ServerGroupInviteResult BuildServerGroupInviteResult(string playerName, InviteResult result)
        {
            return new ServerGroupInviteResult
            {
                GroupId = Id,
                PlayerName = playerName,
                Result = result
            };
        }

        /// <summary>
        /// Build group reuqest to join result
        /// </summary>
        public ServerGroupRequestJoinResult BuildServerGroupRequestJoinResult(string playerName, InviteResult result)
        {
            return new ServerGroupRequestJoinResult
            {
                GroupId = Id,
                PlayerName = playerName,
                Result = result,
                IsJoin = true
            };
        }

        /// <summary>
        /// Build group reuqest to join result
        /// </summary>
        public ServerGroupRequestJoinResult BuildServerGroupReferralResult(string playerName, InviteResult result)
        {
            return new ServerGroupRequestJoinResult
            {
                GroupId = Id,
                PlayerName = playerName,
                Result = result,
                IsJoin = false
            };
        }

        /// <summary>
        /// Build group invite response that is sent to the invitee
        /// </summary>
        public ServerGroupInviteReceived BuildServerGroupInviteReceived()
        {
            var groupMembers = new List<Member>();
            using (membersLock.GetReadLock())
            {
                members.ForEach(m => groupMembers.Add(m.BuildGroupMember()));
            }
            return new ServerGroupInviteReceived
            {
                GroupId = Id,
                GroupMembers = groupMembers
            };
        }

        /// <summary>
        /// Build ready check packet
        /// </summary>
        public ServerGroupSendReadyCheck BuildServerGroupSendReadyCheck(GroupMember member, string message)
        {
            return new ServerGroupSendReadyCheck
            {
                GroupId = Id,
                SenderIdentity = member.Player.BuildTargetPlayerIdentity(),
                Message = message
            };
        }

        /// <summary>
        /// Build group flags changed packet
        /// </summary>
        public ServerGroupFlagsChanged BuildServerGroupFlagsChanged()
        {
            return new ServerGroupFlagsChanged
            {
                GroupId = Id,
                Flags = Flags
            };
        }

        /// <summary>
        /// Build group size change packet
        /// </summary>
        public ServerGroupMaxSizeChange BuildServerGroupMaxSizeChange()
        {
            return new ServerGroupMaxSizeChange
            {
                GroupId = Id,
                MaxSize = MaxSize,
                Flags = Flags
            };
        }

        #endregion
    }
}
