using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Group
{
    public partial class Group
    {
        public delegate IWritable BroadcastCallback(GroupMember member);

        /// <summary>
        /// Broadcast generated message per member
        /// </summary>
        /// <param name="group">Group to broadcast to</param>
        /// <param name="callback">callback to generate message per every member</param>
        public void Broadcast(BroadcastCallback callback)
        {
            Members.ForEach(member => {
                var value = callback(member);
                if (value != null)
                    member.Send(value);
            });
        }

        /// <summary>
        /// Broadcast given message to the whole group
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(IWritable message)
        {
            Members.ForEach(m => m.Send(message));
        }

        /// <summary>
        /// Build Group Join packet for the given member
        /// </summary>
        /// <param name="member">new member who joined</param>
        /// <returns></returns>
        public ServerGroupJoin BuildServerGroupJoin(GroupMember member)
        {
            uint groupIndex = 1;
            var groupMembers = new List<ServerGroupJoin.GroupMemberInfo>();
            foreach (var groupMember in Members)
            {
                groupMembers.Add(groupMember.BuildGroupMemberInfo(groupIndex++));
            }

            return new ServerGroupJoin
            {
                JoinedPlayer = member.BuildTargetPlayerIdentity(),
                GroupId = Id,
                GroupType = Flags,
                MaxSize = MaxSize,
                LootRuleNormal = LootRule.NeedBeforeGreed,         // Under LootThreshold rarity (For Raid)
                LootRuleThreshold = LootRule.RoundRobin,              // This is the selection for Loot Rules in the UI / Over LootTreshold rarity (For Raid)
                LootThreshold = LootThreshold.Excellent,
                LootRuleHarvest = LootRuleHarvest.FirstTagger,      // IDK were it shows this setting in the UI
                GroupMembers = groupMembers,
                LeaderIdentity = PartyLeader.BuildTargetPlayerIdentity(),
                Realm = WorldServer.RealmId
            };
        }

        /// <summary>
        /// Build Member Add packet for the given member
        /// </summary>
        public ServerGroupMemberAdd BuildServerGroupMemberAdd(GroupMember member)
        {
            var groupIndex = (uint)Members.IndexOf(member) + 1;
            var memberInfo = member.BuildGroupMemberInfo(groupIndex);
            return new ServerGroupMemberAdd
            {
                GroupId = Id,
                AddMemberInfo = memberInfo
            };
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
    }
}
