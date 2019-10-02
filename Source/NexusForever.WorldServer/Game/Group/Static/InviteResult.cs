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
        InviteFailedDueToFilledRoles            = 0x000E,
        CannotInviteYourself                    = 0x000F,
        Unknown16                               = 0x0010, // If in group invite gets sent to player, nothing happens on senders screen
        Unknown17                               = 0x0011, // If in group invite gets sent to player, nothing happens on senders screen
        PartyLeaderNotAcceptingRequests         = 0x0012,
        PartyLeaderIsBusyWithOtherRequests      = 0x0013,
        Unknown20                               = 0x0014,
        Unknown21                               = 0x0015, // Invite sent to other player but no UI shows for the sender
        Unknown22                               = 0x0016, // Lua Error if sender is not in group, if inviter is in group it sends another invite with no UI on the senders screen
        PartyInvitePrivilegesHaveBeenRestricted = 0x0017,
        GroupActionFailedDueToPvpFlagSetting    = 0x0018
    }
}
