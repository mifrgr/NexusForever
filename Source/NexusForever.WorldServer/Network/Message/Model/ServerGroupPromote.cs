using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupPromote)]
    public class ServerGroupPromote : IWritable
    {
        public ulong GroupId { get; set; }
        public uint MemberId { get; set; }
        public TargetPlayerIdentity PlayerIdentity { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(MemberId);
            PlayerIdentity.Write(writer);
        }
    }
}