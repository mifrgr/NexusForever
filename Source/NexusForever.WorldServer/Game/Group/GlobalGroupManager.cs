using NexusForever.WorldServer.Game.Entity;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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
        private static readonly ConcurrentQueue<Group> pendingAdd = new ConcurrentQueue<Group>();
        private static readonly ConcurrentQueue<Group> pendingRemove = new ConcurrentQueue<Group>();

        /// <summary>
        /// Next unique ID for a new group.
        /// </summary>
        private static ulong NextGroupId => (uint)Interlocked.Increment(ref nextGroupId);
        private static long nextGroupId;

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
            while (pendingRemove.TryDequeue(out Group group))
                groups.Remove(group);

            groups.ForEach(g => g.Update(lastTick));

            while (pendingAdd.TryDequeue(out Group group))
                groups.Add(group);
        }

        /// <summary>
        /// Create a new group and set given player as party leader
        /// </summary>
        public static Group CreateGroup(Player partyLeader)
        {
            var group = new Group(NextGroupId, partyLeader);
            pendingAdd.Enqueue(group);
            return group;
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
