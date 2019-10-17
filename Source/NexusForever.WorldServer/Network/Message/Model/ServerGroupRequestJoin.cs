using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using static NexusForever.WorldServer.Network.Message.Model.ServerGroupJoin;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupRequestJoin)]
    public class ServerGroupRequestJoin : IWritable
    {
        public ulong GroupId { get; set; }
        public GroupMemberInfo MemberInfo { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            MemberInfo.Write(writer);
        }
    }
}