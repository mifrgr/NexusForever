using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    public static class GroupManager
    {
        private static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static Dictionary<ulong, Group> groupsMap = new Dictionary<ulong, Group>();

        /// <summary>
        /// Id assigned to the next new group
        /// </summary>
        public static ulong NextGroupId => nextGroupId++;

        public static ulong NextGroupMemberId => nextGroupMemberId++;

        private static ulong nextGroupId;

        private static ulong nextGroupMemberId;

        public static void Initialise()
        {
            nextGroupId = 0ul;
            nextGroupMemberId = 0ul;
        }

        public static Group GetGroupById(ulong groupId)
        {
            if (groupsMap.ContainsKey(groupId))
            {
                return groupsMap[groupId];
            }
            return null;
        }

        public static void DismissGroup(Group group)
        {
            groupsMap.Remove(group.GroupId);
        }

        public static Group CreateNewGroup()
        {
            var id = NextGroupId;
            var group = new Group(id);
            groupsMap[id] = group;
            return group;
        }
    }
}
