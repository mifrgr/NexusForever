using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupLeave)]
    public class ClientGroupLeave : IReadable
    {
        public ulong GroupId { get; set; }
        public uint Unk1 { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            Unk1    = reader.ReadUInt(1);
        }
    }
}
