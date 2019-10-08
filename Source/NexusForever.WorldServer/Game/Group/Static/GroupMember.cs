using NexusForever.WorldServer.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Group.Static
{
    /// <summary>
    /// Represent player that is part of the group
    /// </summary>
    public class GroupMember
    {
        public ushort Id;
        public ulong Guid;
        public WorldSession Session;
    }
}
