using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupFlagsChanged)]
    public class ClientGroupFlags : IReadable
    {
        public uint Id { get; set; }
        public uint Flags { get; set; }

        public void Read(GamePacketReader reader)
        {
            Id = reader.ReadUInt();
            Flags = reader.ReadUInt();
        }
    }
}
