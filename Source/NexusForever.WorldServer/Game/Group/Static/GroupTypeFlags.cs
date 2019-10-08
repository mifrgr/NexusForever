using System;

namespace NexusForever.WorldServer.Game.Group.Static
{
    [Flags]
    public enum GroupTypeFlags
    {
        OpenWorld   = 1 << 0,
        Raid        = 1 << 1
    }
}