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

        public static Group GetGroupById(ulong groupId)
        {
            return Groups.Find(group => group.Id == groupId);
        }

        public static void DismissGroup(Group group)
        {
            Groups.Remove(group);
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
        public static GroupInvite FindPlayerInvite(WorldSession session)
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

        public static ServerGroupRemove BuildLeaveGroup(ulong characterId, uint memberId, ulong groupId, RemoveReason reason)
        {
            return new ServerGroupRemove
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
