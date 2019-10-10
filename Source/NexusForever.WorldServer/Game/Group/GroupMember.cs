﻿using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Game.Group
{
    /// <summary>
    /// Represent player that is part of the group
    /// </summary>
    public class GroupMember
    {
        /// <summary>
        /// Unique member ID
        /// </summary>
        public ushort Id;

        /// <summary>
        /// Group that this member belongs to
        /// </summary>
        public Group Group;

        /// <summary>
        /// The player that is the member
        /// </summary>
        public Player Player;

        /// <summary>
        /// Is this member a party leader
        /// </summary>
        public bool IsPartyLeader { get; private set; }

        /// <summary>
        /// Allow memmber to kick
        /// </summary>
        public bool CanKick => (Flags & GroupMemberInfoFlags.CanKick) != 0;

        /// <summary>
        /// Allow member to invite
        /// </summary>
        public bool CanInvite => (Flags & GroupMemberInfoFlags.CanInvite) != 0;

        /// <summary>
        /// Allow member to mark
        /// </summary>
        public bool CanMark => (Flags & GroupMemberInfoFlags.CanMark) != 0;

        /// <summary>
        /// Allow doing ready check
        /// </summary>
        public bool CanReadyCheck =>
            IsPartyLeader || (Flags & GroupMemberInfoFlags.CanReadyCheck) != 0;

        /// <summary>
        /// Can this member adjust flags?
        /// </summary>
        public bool CanUpdateFlags =>
            IsPartyLeader || (Flags & GroupMemberInfoFlags.RaidAssistant) != 0;

        /// <summary>8
        /// Generate Info flags that can be sent to the client.
        /// </summary>
        public GroupMemberInfoFlags Flags
        {
            get
            {
                var flags = _flags;
                if (IsPartyLeader)
                    flags |= GroupMemberInfoFlags.GroupAdminFlags;
                else
                    flags |= GroupMemberInfoFlags.GroupMemberFlags;

                if ((flags & GroupMemberInfoFlags.RaidAssistant) != 0)
                    flags |= GroupMemberInfoFlags.GroupAssistantFlags;

                if ((flags & GroupMemberInfoFlags.MainTank) != 0)
                {
                    flags |= GroupMemberInfoFlags.MainTankFlags;
                    flags &= ~GroupMemberInfoFlags.RoleFlags;
                    flags |= GroupMemberInfoFlags.Tank;
                }

                if ((flags & GroupMemberInfoFlags.MainAssist) != 0)
                    flags |= GroupMemberInfoFlags.MainAssistFlags;

                return flags;
            }
        }

        private GroupMemberInfoFlags _flags;

        /// <summary>
        /// Mark this member as party leader
        /// </summary>
        public void SetIsPartyLeader(bool isPartyLead)
        {
            IsPartyLeader = isPartyLead;
        }

        /// <summary>
        /// Toggle flags on/off.
        /// </summary>
        public void SetFlags(GroupMemberInfoFlags flags, bool value)
        {
            if (value && (flags & GroupMemberInfoFlags.RoleFlags) != 0)
                _flags &= ~GroupMemberInfoFlags.RoleFlags;

            if (value)
                _flags |= flags;
            else
                _flags &= ~flags;
        }
    }
}
