using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupReferral)]
    public class ServerGroupReferral : IWritable
    {
        public ulong GroupId { get; set; }
        public TargetPlayerIdentity inviter { get; set; }
        public string inviteeName { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            inviter.Write(writer);
            writer.WriteStringWide(inviteeName);
        }
    }
}