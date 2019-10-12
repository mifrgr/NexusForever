using NexusForever.WorldServer.Game.Entity;
using System;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    /// <summary>
    /// Coordinate in game groups.
    /// </summary>
    public static class GlobalGroupManager
    {
        // TODO: move this to the config file
        private const double ClearInvitesInterval = 1d;

        /// <summary>
        /// List of active groups
        /// </summary>
        private readonly static List<Group> groups = new List<Group>();

        /// <summary>
        /// Next unique ID for a new group
        /// </summary>
        private static ulong nextGroupId;

        /// <summary>
        /// Indicates if currently is in the update cycle.
        /// </summary>
        private static bool inUpdateCycle;

        /// <summary>
        /// Used for throttling the cleanup rate
        /// </summary>
        private static double timeToClearInvites = ClearInvitesInterval;

        /// <summary>
        /// Collect groups that need to be removed during update cycle
        /// </summary>
        private readonly static List<Group> removeGroups = new List<Group>();

        /// <summary>
        /// Set things up
        /// </summary>
        public static void Initialise()
        {
            nextGroupId = 1;
            inUpdateCycle = false;
        }

        /// <summary>
        /// Trigger group house keeping
        /// </summary>
        public static void Update(double lastTick)
        {
            // clear pending invites.
            timeToClearInvites -= lastTick;
            if (timeToClearInvites <= 0d)
            {
                timeToClearInvites = ClearInvitesInterval;

                inUpdateCycle = true;
                var now = DateTime.UtcNow;
                foreach (var group in groups)
                    if (group.HasPendingInvites)
                        group.ClearExpiredInvites(now);
                inUpdateCycle = false;

                removeGroups.ForEach(g => groups.Remove(g));
                removeGroups.Clear();
            }
        }

        /// <summary>
        /// Create a new group and set given player as party leader
        /// </summary>
        public static Group CreateGroup(Player partyLeader)
        {
            var group = new Group(nextGroupId++, partyLeader);
            groups.Add(group);
            return group;
        }

        /// <summary>
        /// Disband the group, remove all members and invites from it
        /// </summary>
        public static void RemoveGroup(Group group)
        {
            if (inUpdateCycle)
                removeGroups.Add(group);
            else
                groups.Remove(group);
        }
    }
}
