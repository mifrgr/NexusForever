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
        /// Can this player invite?
        /// </summary>
        public bool canInvite => isPartyLead || allowInvite;

        /// <summary>
        /// Is this member a party leader
        /// </summary>
        public bool isPartyLead;

        /// <summary>
        /// Allow memmber to kick
        /// </summary>
        public bool allowKick;

        /// <summary>
        /// Allow member to invite
        /// </summary>
        public bool allowInvite;

        /// <summary>
        /// Allow member to mark
        /// </summary>
        public bool allowMarking;

        /// <summary>
        /// Generate Info flags that can be sent to the client.
        /// </summary>
        public GroupMemberInfoFlags InfoFlags
        {
            get
            {
                var flags = isPartyLead
                          ? GroupMemberInfoFlags.GroupAdmin
                          : GroupMemberInfoFlags.GroupMember;
                if (allowInvite)
                    flags |= GroupMemberInfoFlags.CanInvite;
                if (allowKick)
                    flags |= GroupMemberInfoFlags.CanKick;
                if (allowMarking)
                    flags |= GroupMemberInfoFlags.CanMark;
                return flags;
            }
        }
    }
}
