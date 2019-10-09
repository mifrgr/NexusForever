using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network;
using System.Collections.Generic;
using System.Linq;

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
        public static List<Group> Groups = new List<Group>();

        /// <summary>
        /// Unique ID for the next new group
        /// </summary>
        public static ulong NextGroupId => nextGroupId++;

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
            Groups.Add(group);
            return group;
        }

        /// <summary>
        /// Disband the group and remove all members from it
        /// </summary>
        public static void DisbandGroup(Group group)
        {
            var members = group.Members.ToList();
            foreach (var member in members)
                group.RemoveMember(member);
            Groups.Remove(group);
        }
    }
}
