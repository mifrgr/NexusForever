using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Group.Static
{
    [Flags]
    public enum GroupMemberInfoFlags
    {
        CanInvite       = 1 << 1,
        CanKick         = 1 << 2,
        Disconnected    = 1 << 3,
        CanMark         = 1 << 13,
        // RaidAssistant    = ?
        // MainTank         = ?
        // MainAssist       = ?
        // RoleLocked       = ?

        GroupMember     = CanMark,
        GroupAdmin      = CanInvite | CanKick | CanMark
    }
}
