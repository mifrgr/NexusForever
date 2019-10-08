using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupJoin)]
    public class ServerGroupJoin : IWritable
    {
        public class GroupMemberInfo : IWritable
        {
            public TargetPlayerIdentity MemberIdentity { get; set; }
            public GroupMemberInfoFlags Flags { get; set; }
            public Member GroupMember { get; set; } = new Member();
            public uint GroupIndex { get; set; }

            public void Write(GamePacketWriter writer)
            {
                MemberIdentity.Write(writer);
                writer.Write((uint)Flags);
                GroupMember.Write(writer);
                writer.Write(GroupIndex);
            }
        }

        public class UnknownStruct0 : IWritable
        {
            public uint Unknown8 { get; set; }
            public uint Unknown9 { get; set; }

            public void Write(GamePacketWriter writer)
            {
                writer.Write(Unknown8);
                writer.Write(Unknown9);
            }
        }

        public TargetPlayerIdentity JoinedPlayer { get; set; } = new TargetPlayerIdentity();
        public ulong GroupId { get; set; }
        public GroupTypeFlags GroupType { get; set; }
        public uint MaxSize { get; set; }
        public LootRule LootRuleNormal { get; set; } // 3
        public LootRule LootRuleThreshold { get; set; } // 3
        public LootThreshold LootThreshold { get; set; } // 4
        public LootRuleHarvest LootRuleHarvest { get; set; } // 2

        public List<GroupMemberInfo> GroupMembers { get; set; } = new List<GroupMemberInfo>();

        public TargetPlayerIdentity LeaderIdentity { get; set; } = new TargetPlayerIdentity();
        public ushort Realm { get; set; } = WorldServer.RealmId;

        public List<UnknownStruct0> UnknownStruct0List { get; set; } = new List<UnknownStruct0>();

        public void Write(GamePacketWriter writer)
        {
            JoinedPlayer.Write(writer);
            writer.Write(GroupId);
            writer.Write(GroupType, 32u);
            writer.Write(GroupMembers.Count, 32u);
            writer.Write(MaxSize);
            writer.Write(LootRuleNormal, 3u);
            writer.Write(LootRuleThreshold, 3u);
            writer.Write(LootThreshold, 4u);
            writer.Write(LootRuleHarvest, 2u);

            GroupMembers.ForEach(i => i.Write(writer));

            LeaderIdentity.Write(writer);
            writer.Write(Realm, 14u);

            writer.Write(UnknownStruct0List.Count, 32u);
            UnknownStruct0List.ForEach(i => i.Write(writer));
        }
    }
}
