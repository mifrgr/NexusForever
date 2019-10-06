using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupSetRole)]
    public class ClientGroupSetRole : IReadable
    {
        public ulong GroupId { get; set; }
        public TargetPlayerIdentity PlayerIdentity { get; set; } = new TargetPlayerIdentity();
        public uint Unk1 { get; set; }
        public uint Unk2 { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId     = reader.ReadULong();
            PlayerIdentity.Read(reader);
            Unk1        = reader.ReadUInt();
            Unk2        = reader.ReadUInt();
        }
    }
}
