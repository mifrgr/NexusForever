using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;

namespace NexusForever.WorldServer.Game.Group
{
    /// <summary>
    /// Represent player that is part of the group
    /// </summary>
    public partial class GroupMember
    {
        /// <summary>
        /// Unique member ID
        /// </summary>
        public readonly ushort Id;

        /// <summary>
        /// Group that this member belongs to
        /// </summary>
        public readonly Group Group;

        /// <summary>
        /// The player that is the member
        /// </summary>
        public readonly Player Player;

        /// <summary>
        /// Member flags. Use Flags accessor to get correct flags
        /// </summary>
        private GroupMemberInfoFlags flags;

        /// <summary>
        /// Is this member a party leader
        /// </summary>
        public bool IsPartyLeader => Group.PartyLeader?.Id == Id;

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

        /// <summary>8
        /// Generate Info flags that can be sent to the client.
        /// </summary>
        public GroupMemberInfoFlags Flags
        {
            get
            {
                var flags = this.flags;
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

        /// <summary>
        /// Create an instance of GroupMember
        /// </summary>
        public GroupMember(ushort id, Group group, Player player)
        {
            Id = id;
            Group = group;
            Player = player;
            flags = 0;
        }

        /// <summary>
        /// Can this member update given flags for the given member?
        /// </summary>
        /// <param name="updateFlags">flags trying to update</param>
        /// <param name="other">member whose flags are being modified</param>
        /// <returns></returns>
        public bool CanUpdateFlags(GroupMemberInfoFlags updateFlags, GroupMember other)
        {
            if (IsPartyLeader)
                return true;

            if ((Flags & GroupMemberInfoFlags.RaidAssistant) != 0)
                return true;

            if (other.Id != Id)
                return false;

            var allowedFlags = GroupMemberInfoFlags.RoleFlags
                             | GroupMemberInfoFlags.HasSetReady
                             | GroupMemberInfoFlags.Ready;
            return (updateFlags & allowedFlags) == updateFlags;
        }

        /// <summary>
        /// Clear ready check related flags
        /// </summary>
        public void PrepareForReadyCheck()
        {
            var flags = GroupMemberInfoFlags.HasSetReady
                      | GroupMemberInfoFlags.Ready;
            this.flags &= ~flags;
            this.flags |= GroupMemberInfoFlags.Pending;
        }

        /// <summary>
        /// Toggle flags on/off.
        /// </summary>
        public void SetFlags(GroupMemberInfoFlags flags, bool value)
        {
            if (value && (flags & GroupMemberInfoFlags.RoleFlags) != 0)
                this.flags &= ~GroupMemberInfoFlags.RoleFlags;

            if (value && (flags & GroupMemberInfoFlags.HasSetReady) != 0)
                this.flags &= ~GroupMemberInfoFlags.Pending;

            if (value)
                this.flags |= flags;
            else
                this.flags &= ~flags;
        }
    }
}
