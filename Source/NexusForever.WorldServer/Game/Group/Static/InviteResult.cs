using System;
using System.Collections.Generic;
using System.Text;

namespace NexusForever.WorldServer.Game.Group.Static
{
    public enum InviteResult
    {
        Sent                                = 0x0000,
        Declined                            = 0x0001,
        Accepted                            = 0x0002,
        NotPermitted                        = 0x0003,
        Unknown4                            = 0x0004,
        PlayerNotFound                      = 0x0005,
        PlayerAlreadyInGroup                = 0x0006,
        PlayerHasPendingInvite              = 0x0007,
        PlayerInvitateHasExpired            = 0x0008,
        PlayerAlreadyInvited                = 0x0009,
        InviteExpired                       = 0x000A,
        AlreadyInvited                      = 0x000B,
        InvalidRealmName                    = 0x000C,
        GroupIsFull                         = 0x000D,
        InviteFailedDueToFilledRoles        = 0x000E,
        CannotInviteYourself                = 0x000F,
        Unknown16                           = 0x0010,
        Unknown17                           = 0x0011,
        PartyLeaderNotAcceptingRequests     = 0x0012,
        PartyLeaderIsBusyWithOtherRequests  = 0x0013
    }
}
