using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;
using System.Numerics;

namespace NexusForever.WorldServer.Network.Message.Model
{
    [Message(GameMessageOpcode.ServerGroupPositionUpdate)]
    public class ServerGroupPositionUpdate : IWritable
    {
        public ulong GroupId { get; set; }
        public ushort WorldZoneId { get; set; }
        public List<TargetPlayerIdentity> Players { get; set; }
        public List<Position> Positions { get; set; }
        public List<uint> Unknown { get; set; }
        public List<uint> Flags { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(GroupId);
            writer.Write(WorldZoneId, 15u);
            writer.Write(Players.Count, 32u);
            
            Players.ForEach(p => p.Write(writer));
            Positions.ForEach(p => p.Write(writer));
            Unknown.ForEach(u => writer.Write(u));
            Flags.ForEach(f => writer.Write(f));
        }
    }
}