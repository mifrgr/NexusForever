using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Group.Static
{
    public enum InviteResult
    {
        Sent                                    = 0x0000,
        Declined                                = 0x0001,
        Accepted                                = 0x0002,
        NotPermitted                            = 0x0003,
        Unknown4                                = 0x0004, // If in group invite gets sent to player, nothing happens on senders screen
        PlayerNotFound                          = 0x0005,
        PlayerAlreadyInGroup                    = 0x0006,
        PlayerHasPendingInvite                  = 0x0007,
        PlayerInvitateHasExpired                = 0x0008,
        PlayerAlreadyInvited                    = 0x0009,
        InviteExpired                           = 0x000A,
        AlreadyInvited                          = 0x000B,
        InvalidRealmName                        = 0x000C,
        GroupIsFull                             = 0x000D,
        RoleFull                                = 0x000E,
        CannotInviteYourself                    = 0x000F,
        ServerControlled                        = 0x0010,
        GroupNotFound                           = 0x0011,
        NotAcceptingRequests                    = 0x0012,
        Busy                                    = 0x0013,
        SentToLeader                            = 0x0014,
        LeaderOffline                           = 0x0015,
        WrongFaction                            = 0x0016,
        PrivilegeRestricted                     = 0x0017,
        PvPFlagRestriction                      = 0x0018
    }
}
