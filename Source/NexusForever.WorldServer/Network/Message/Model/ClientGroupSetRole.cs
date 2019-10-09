using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ClientGroupSetRole)]
    public class ClientGroupSetRole : IReadable
    {
        public ulong GroupId { get; set; }
        public TargetPlayerIdentity PlayerIdentity { get; set; } = new TargetPlayerIdentity();
        public GroupMemberInfoFlags Flags { get; set; }
        public GroupMemberInfoFlags ChangedFlag { get; set; }

        public void Read(GamePacketReader reader)
        {
            GroupId = reader.ReadULong();
            PlayerIdentity.Read(reader);
            Flags = (GroupMemberInfoFlags)reader.ReadUInt();
            ChangedFlag = (GroupMemberInfoFlags)reader.ReadUInt();
        }
    }
}
