using NexusForever.WorldServer.Game.Entity;
using System;
using System.Collections.Concurrent;
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
        /// Used for throttling the cleanup rate
        /// </summary>
        private static double timeToClearInvites = ClearInvitesInterval;

        private static readonly ConcurrentQueue<Group> pendingAdd = new ConcurrentQueue<Group>();
        private static readonly ConcurrentQueue<Group> pendingRemove = new ConcurrentQueue<Group>();

        private static readonly object groupLock = new object();

        /// <summary>
        /// Set things up
        /// </summary>
        public static void Initialise()
        {
            nextGroupId = 1;
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
                var now = DateTime.UtcNow;
                foreach (var group in groups)
                    if (group.HasPendingInvites)
                        group.ClearExpiredInvites(now);
            }

            while (pendingRemove.TryDequeue(out Group group))
                groups.Remove(group);

            while (pendingAdd.TryDequeue(out Group group))
                groups.Add(group);
        }

        /// <summary>
        /// Create a new group and set given player as party leader
        /// </summary>
        public static Group CreateGroup(Player partyLeader)
        {
            lock (groupLock)
            {
                var group = new Group(nextGroupId++, partyLeader);
                pendingAdd.Enqueue(group);
                return group;
            }
        }

        /// <summary>
        /// Disband the group, remove all members and invites from it
        /// </summary>
        public static void RemoveGroup(Group group)
        {
            pendingRemove.Enqueue(group);
        }
    }
}
