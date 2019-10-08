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

        public static void Initialise()
        {
            nextGroupId = 1;
            nextGroupMemberId = 1;
        }

        public static Group GetGroupById(ulong groupId)
        {
            return Groups.Find(group => group.Id == groupId);
        }

        public static void DismissGroup(Group group)
        {
            Groups.Remove(group);
        }

        /// <summary>
        /// Return Group player is part of or null
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static Group FindPlayerGroup(WorldSession session)
        {
            return Groups.Find(
                group => group.Members.Exists(
                    member => member.Guid == session.Player.Guid
                )
            );
        }

        /// <summary>
        /// Return Group player has been invited to, or null
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static Group.Invite FindPlayerInvite(WorldSession session)
        {
            foreach (var group in Groups)
            {
                var invite = group.FindInvite(session);
                if (invite != null)
                    return invite;
            }
            return null;
        }

        public static Group CreateGroup()
        {
            var group = new Group
            {
                Id = NextGroupId
            };
            Groups.Add(group);
            return group;
        }

        public static void DisbandGroup(Group group)
        {
            Groups.Remove(group);
        }

        public static ServerGroupLeave BuildLeaveGroup(ulong characterId, uint memberId, ulong groupId, RemoveReason reason)
        {
            return new ServerGroupLeave
            {
                GroupId = groupId,
                MemberId = memberId,
                PlayerLeave = new TargetPlayerIdentity()
                {
                    RealmId = WorldServer.RealmId,
                    CharacterId = characterId
                },
                RemoveReason = reason
            };
        }
    }
}
