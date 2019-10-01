using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Social;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupInvite)]
    public class ClientGroupInvite : IReadable
    {
        public string PlayerName { get; private set; }
        public string Unknown0 { get; set; }

        public void Read(GamePacketReader reader)
        {
            PlayerName = reader.ReadWideString();
            Unknown0 = reader.ReadWideString();
        }
    }
}
