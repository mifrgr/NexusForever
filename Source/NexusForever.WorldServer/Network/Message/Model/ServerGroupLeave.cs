using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupRemove)]
    class ServerGroupLeave : IWritable
    {
        public ulong GroupId { get; set; }
        public uint Unknown1 { get; set; }
        // public uint Unknown2 { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Unknown1);
            // writer.Write(Unknown2);
        }
    }
}
