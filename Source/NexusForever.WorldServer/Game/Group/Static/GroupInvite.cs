using NexusForever.WorldServer.Network;

namespace NexusForever.WorldServer.Game.Group.Static
{
    /// <summary>
    /// Represent player that has been invited to the group,
    /// but is not yet a member. Waiting for accept.
    /// 
    /// Players can either be invited, or request to join (TODO)
    /// </summary>
    public class GroupInvite
    {
        public ulong Guid;
        public WorldSession Session;

        /// <summary>
        /// Member who invited
        /// </summary>
        public GroupMember Inviter;
    }
}
