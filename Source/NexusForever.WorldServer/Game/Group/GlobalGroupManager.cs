using NexusForever.Shared;
using NexusForever.WorldServer.Game.Entity;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    /// <summary>
    /// Coordinate in game groups.
    /// </summary>
    public sealed class GlobalGroupManager : Singleton<GlobalGroupManager>, IUpdate
    {
        /// <summary>
        /// List of active groups
        /// </summary>
        private readonly List<Group> groups = new List<Group>();
        private readonly ConcurrentQueue<Group> pendingAdd = new ConcurrentQueue<Group>();
        private readonly ConcurrentQueue<Group> pendingRemove = new ConcurrentQueue<Group>();

        /// <summary>
        /// Next unique ID for a new group.
        /// </summary>
        private ulong NextGroupId => (ulong)Interlocked.Increment(ref nextGroupId);
        private long nextGroupId;

        /// <summary>
        /// Set things up
        /// </summary>
        public void Initialise()
        {
            nextGroupId = 1;
        }

        /// <summary>
        /// Trigger group house keeping
        /// </summary>
        public void Update(double lastTick)
        {
            while (pendingRemove.TryDequeue(out Group? group))
                groups.Remove(group);

            groups.ForEach(g => g.Update(lastTick));

            while (pendingAdd.TryDequeue(out Group? group))
                groups.Add(group);
        }

        /// <summary>
        /// Create a new group and set given player as party leader
        /// </summary>
        public Group CreateGroup(Player partyLeader)
        {
            var group = new Group(NextGroupId, partyLeader);
            pendingAdd.Enqueue(group);
            return group;
        }

        /// <summary>
        /// Disband the group, remove all members and invites from it
        /// </summary>
        public void RemoveGroup(Group group)
        {
            pendingRemove.Enqueue(group);
        }
    }
}
