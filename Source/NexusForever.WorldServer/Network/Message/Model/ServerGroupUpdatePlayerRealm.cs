using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupUpdatePlayerRealm)]
    public class ServerGroupUpdatePlayerRealm : IWritable
    {
        public ulong GroupId { get; set; }
        public TargetPlayerIdentity TargetPlayer { get; set; }

        public ushort Realm { get; set; }
        public ushort WorldZoneId { get; set; }
        public uint Unknown25 { get; set; }
        public uint Unknown26 { get; set; }
        public bool SyncedToGroup { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            TargetPlayer.Write(writer);

            writer.Write(Realm, 14u);
            writer.Write(WorldZoneId, 15u);
            writer.Write(Unknown25);        // phases?
            writer.Write(Unknown26);        // phases?
            writer.Write(SyncedToGroup);

        }
    }
}
