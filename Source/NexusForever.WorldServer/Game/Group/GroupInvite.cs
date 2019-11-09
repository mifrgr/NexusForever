using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using System;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    /// <summary>
    /// Represent player that has been invited to the group,
    /// but is not yet a member. Waiting for accept.
    /// 
    /// Players can either be invited, or request to join (TODO)
    /// </summary>
    public sealed partial class GroupInvite
    {
        /// <summary>
        /// Invite timeout duration
        /// </summary>
        public const double InviteTimeout = 30d;

        /// <summary>
        /// Group that this invite belongs to
        /// </summary>
        public readonly Group Group;

        /// <summary>
        /// Player being invited
        /// </summary>
        public readonly Player Player;

        /// <summary>
        /// Member who invited
        /// </summary>
        public readonly GroupMember Inviter;

        /// <summary>
        /// Type of invite
        /// </summary>
        public readonly GroupInviteType Type;

        /// <summary>
        /// Time when this invite expires
        /// </summary>
        public readonly DateTime ExpirationTime = DateTime.UtcNow.AddSeconds(InviteTimeout);

        /// <summary>
        /// Create invite to group
        /// </summary>
        public GroupInvite(Group group, Player player, GroupMember inviter, GroupInviteType type)
        {
            Group = group;
            Player = player;
            Inviter = inviter;
            Type = type;
        }
    }
}
