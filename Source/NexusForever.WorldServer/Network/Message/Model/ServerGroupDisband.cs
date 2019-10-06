using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupDisband)]
    public class ServerGroupDisband : IWritable
    {
        public ulong GroupId { get; set; }
        public uint MemberId { get; set; }
        public TargetPlayerIdentity PlayerLeave { get; set; }
        public RemoveReason DisbandReason { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(MemberId);
            PlayerLeave.Write(writer);
            writer.Write(DisbandReason, 4);
        }
    }
}
