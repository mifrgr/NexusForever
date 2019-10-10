using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity.Static;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupSendReadyCheck)]
    public class ServerGroupSendReadyCheck : IWritable
    {
        public ulong GroupId { get; set; }
        public TargetPlayerIdentity SenderIdentity = new TargetPlayerIdentity();
        public string Message { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            SenderIdentity.Write(writer);
            writer.WriteStringWide(Message);
        }
    }
}
