using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
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
            public uint Unknown7 { get; set; }
            public GroupMember GroupMember { get; set; } = new GroupMember();
            public uint GroupIndex { get; set; }

            public void Write(GamePacketWriter writer)
            {
                MemberIdentity.Write(writer);
                writer.Write(Unknown7);
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

        public TargetPlayerIdentity PlayerJoined { get; set; } = new TargetPlayerIdentity();
        public ulong GroupId { get; set; }
        public uint Unknown0 { get; set; }
        public uint Unknown1 { get; set; }
        public byte Unknown3 { get; set; } // 3
        public byte Unknown4 { get; set; } // 3
        public byte Unknown5 { get; set; } // 4
        public byte Unknown6 { get; set; } // 2

        public List<GroupMemberInfo> GroupMembers { get; set; } = new List<GroupMemberInfo>();

        public TargetPlayerIdentity LeaderIdentity { get; set; } = new TargetPlayerIdentity();
        public ushort Realm { get; set; } = WorldServer.RealmId;

        public List<UnknownStruct0> UnknownStruct0List { get; set; } = new List<UnknownStruct0>();

        public void Write(GamePacketWriter writer)
        {
            PlayerJoined.Write(writer);
            writer.Write(GroupId);
            writer.Write(Unknown0);
            writer.Write(GroupMembers.Count, 32u);
            writer.Write(Unknown1);
            writer.Write(Unknown3, 3u);
            writer.Write(Unknown4, 3u);
            writer.Write(Unknown5, 4u);
            writer.Write(Unknown6, 2u);

            GroupMembers.ForEach(i => i.Write(writer));

            LeaderIdentity.Write(writer);
            writer.Write(Realm, 14u);

            writer.Write(UnknownStruct0List.Count, 32u);
            UnknownStruct0List.ForEach(i => i.Write(writer));
        }
    }
}
