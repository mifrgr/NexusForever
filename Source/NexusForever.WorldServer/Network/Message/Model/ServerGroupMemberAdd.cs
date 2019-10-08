using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using static NexusForever.WorldServer.Network.Message.Model.ServerGroupJoin;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupMemberAdd)]
    public class ServerGroupMemberAdd : IWritable
    {
        public ulong GroupId { get; set; }
        public ushort Unknown0 { get; set; }
        public GroupMemberInfo AddMemberInfo { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(Unknown0);
            AddMemberInfo.Write(writer);
        }
    }
}