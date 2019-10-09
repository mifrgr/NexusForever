using NexusForever.WorldServer.Game.Entity;

namespace NexusForever.WorldServer.Game.Group
{
    /// <summary>
    /// Represent player that has been invited to the group,
    /// but is not yet a member. Waiting for accept.
    /// 
    /// Players can either be invited, or request to join (TODO)
    /// </summary>
    public class GroupInvite
    {
        /// <summary>
        /// Group that this invite belongs to
        /// </summary>
        public Group Group;

        /// <summary>
        /// Player being invited
        /// </summary>
        public Player Player;

        /// <summary>
        /// Member who invited
        /// </summary>
        public GroupMember Inviter;
    }
}
