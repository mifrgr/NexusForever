using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Game.Group
{
    public static class GroupManager
    {
        private static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        private static Dictionary<ulong, Group> groupsMap = new Dictionary<ulong, Group>();
        private static Dictionary<ulong, ulong> groupChar = new Dictionary<ulong, ulong>();

        /// <summary>
        /// Id assigned to the next new group
        /// </summary>
        public static ulong NextGroupId => nextGroupId++;
        public static ulong NextGroupMemberId => nextGroupMemberId++;
        public static uint NextGroupIndex => nextGroupIndex++;
        public static Dictionary<ulong, Group.Member> GroupMember = new Dictionary<ulong, Group.Member>();

        private static ulong nextGroupId;
        private static ulong nextGroupMemberId;
        private static uint nextGroupIndex;

        public static void Initialise()
        {
            nextGroupId = 1ul;
            nextGroupMemberId = 1ul;
            nextGroupIndex = 1u;
        }

        public static Group GetGroupById(ulong groupId)
        {
            if (groupsMap.ContainsKey(groupId))
                return groupsMap[groupId];

            return null;
        }

        public static void DismissGroup(Group group)
        {
            groupsMap.Remove(group.GroupId);

            foreach (var item in groupChar.Where(gc => gc.Value == group.GroupId).ToList())
            {
                groupChar.Remove(item.Key);
            }
        }

        public static Group CreateNewGroup(ulong playerGuid)
        {
            var id = NextGroupId;
            var group = new Group(id);

            if (!groupChar.ContainsKey(playerGuid))
            {
                groupsMap.Add(id, group);
                groupChar.Add(playerGuid, id);
            }
            else
            {
                id = groupChar[playerGuid];
                return groupsMap[id];
            }

            return group;
        }

        public static void SendGroupRemove(WorldSession localSession, WorldSession targetSession, Group group, ulong memberId, RemoveReason reason)
        {
            var groupLeave = new ServerGroupLeave
            {
                GroupId     = group.GroupId,
                MemberId    = (uint)memberId,
                PlayerLeave = new TargetPlayerIdentity()
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = localSession.Player.Guid
                },
                RemoveReason = reason
            };

            localSession.EnqueueMessageEncrypted(groupLeave);
            targetSession.EnqueueMessageEncrypted(groupLeave);
        }

        public static void RemoveGroup(Group group)
        {
            // Remove Group and Members
            foreach (var groupMember in group.Members.ToList())
                group.RemoveMember(groupMember);

            if (group.IsEmpty)
                DismissGroup(group);
        }
    }
}
