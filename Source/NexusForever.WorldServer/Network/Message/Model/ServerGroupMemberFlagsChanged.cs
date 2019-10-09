using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupMemberFlagsChanged)]
    public class ServerGroupMemberFlagsChanged : IWritable
    {
        public ulong GroupId { get; set; }
        public uint MemberId { get; set; }
        public TargetPlayerIdentity PlayerIdentity { get; set; }
        public GroupMemberInfoFlags Flags { get; set; }
        public bool FromPromotion { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(MemberId);
            PlayerIdentity.Write(writer);
            writer.Write(Flags, 32u);
            writer.Write(FromPromotion);
        }
    }
}