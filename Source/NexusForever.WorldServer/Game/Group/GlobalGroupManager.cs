using NexusForever.WorldServer.Game.Entity;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    /// <summary>
    /// Coordinate in game groups.
    /// </summary>
    public static class GlobalGroupManager
    {
        /// <summary>
        /// List of active groups
        /// </summary>
        private readonly static List<Group> groups = new List<Group>();

        /// <summary>
        /// Unique ID for the next new group
        /// </summary>
        private static ulong NextGroupId => nextGroupId++;

        /// <summary>
        /// Unique ID for the next new group member
        /// </summary>
        public static ushort NextGroupMemberId => nextGroupMemberId++;

        private static ulong nextGroupId;
        private static ushort nextGroupMemberId;

        private static double InviteToExpire = 30d;

        public static void Initialise()
        {
            nextGroupId = 1;
            nextGroupMemberId = 1;
        }

        public static void Update(double lastTick)
        {
            InviteToExpire -= lastTick;
            if (InviteToExpire <= 0d)
            {
                /// @TODO: Make them tickets dissapear like my dad when I was 4 years old
                InviteToExpire = 30d;
            }
        }

        /// <summary>
        /// Create a new group and set given player as party leader
        /// </summary>
        public static Group CreateGroup(Player partyLeader)
        {
            var group = new Group(NextGroupId, partyLeader);
            groups.Add(group);
            return group;
        }

        /// <summary>
        /// Disband the group, remove all members and invites from it
        /// </summary>
        public static void RemoveGroup(Group group)
        {
            groups.Remove(group);
        }
    }
}
