using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Game.Social;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupRequestJoinResponse)]
    public class ClientGroupRequestJoinResponse : IReadable
    {
        public ulong GroupId { get; set; }
        public bool Accepted { get; set; }
        public string PlayerName { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            Accepted = reader.ReadBit();
            PlayerName = reader.ReadWideString();
        }
    }
}
