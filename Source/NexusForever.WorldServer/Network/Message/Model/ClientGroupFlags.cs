using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupFlagsChanged)]
    public class ClientGroupFlags : IReadable
    {
        public ulong GroupId { get; set; }
        public GroupFlags Flag { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            Flag = (GroupFlags)reader.ReadUInt();
        }
    }
}
