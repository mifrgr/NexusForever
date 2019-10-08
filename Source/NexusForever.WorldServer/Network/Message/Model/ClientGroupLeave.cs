using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupLeave)]
    public class ClientGroupLeave : IReadable
    {
        public ulong GroupId { get; set; }
        public GroupLeaveScope Scope { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            Scope = (GroupLeaveScope)reader.ReadUInt(1);
        }
    }
}
