using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupPositionUpdate)]
    public class ServerGroupPositionUpdate : IWritable
    {
        public struct Entry
        {
            public TargetPlayerIdentity Player;
            public Position Position;
            public uint UnitId;         // ? guess
            public uint Flags;          // ? guess
        }

        public ulong GroupId { get; set; }
        public ushort WorldZoneId { get; set; }
        public Entry[] Entries { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(WorldZoneId, 15u);
            writer.Write(Entries.Length, 32u);

            foreach (var entry in Entries)
                entry.Player.Write(writer);

            foreach (var entry in Entries)
                entry.Position.Write(writer);

            foreach (var entry in Entries)
                writer.Write(entry.UnitId);

            foreach (var entry in Entries)
                writer.Write(entry.Flags);
        }
    }
}