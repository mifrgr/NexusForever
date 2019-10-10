using NexusForever.WorldServer.Game.Entity;
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
        /// Generate Info flags that can be sent to the client.
        /// </summary>
        public GroupMemberInfoFlags Flags { get; private set; }

        /// <summary>
        /// Mark this member as party leader
        /// </summary>
        public void SetIsPartyLeader(bool isPartyLead)
        {
            IsPartyLeader = isPartyLead;
            if (IsPartyLeader)
            {
                Flags |= GroupMemberInfoFlags.GroupAdmin;
            }
            else
            {
                Flags &= ~GroupMemberInfoFlags.GroupAdmin;
                Flags |= GroupMemberInfoFlags.GroupMember;
            }
        }

        /// <summary>
        /// Toggle permission flags to on or off according to value
        /// </summary>
        public void SetPermissonFlags(GroupMemberInfoFlags flag, bool value)
        {
            var allowed = GroupMemberInfoFlags.GroupAdmin;
            var cleanFlags = flag & allowed;

            if (value)
                Flags |= cleanFlags;
            else
                Flags &= ~cleanFlags;
        }
    }
}
