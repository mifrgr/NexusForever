using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupInviteReceived)]
    public class ServerGroupInviteReceived : IWritable
    {
        public ulong GroupId { get; set; }
        public uint Unknown0 { get; set; }
        public uint Unknown1 { get; set; }

        public List<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Unknown0);
            writer.Write(Unknown1);

            writer.Write(GroupMembers.Count, 32u);
            GroupMembers.ForEach(i => i.Write(writer));
        }
    }
}
