using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerUpdatePhase)]
    public class ServerUpdatePhase : IWritable
    {
        public uint CanSee { get; set; }
        public uint CanSeeMe { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(CanSee);
            writer.Write(CanSeeMe);
        }
    }
}
