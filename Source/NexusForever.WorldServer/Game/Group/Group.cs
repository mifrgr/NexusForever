using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group
    {
        /// <summary>
        /// Unique Group ID
        /// </summary>
        public ulong Id;

        /// <summary>
        /// True of group has no other members aside from party leader in it
        /// </summary>
        public bool IsEmpty => Members.Count <= 1;

        /// <summary>
        /// Current party leader
        /// </summary>
        public GroupMember PartyLeader;

        /// <summary>
        /// Group members
        /// </summary>
        public List<GroupMember> Members { get; } = new List<GroupMember>();

        /// <summary>
        /// Players who have been invited or who have request to join the group
        /// </summary>
        public List<GroupInvite> Invites { get; } = new List<GroupInvite>();

        /// <summary>
        /// Create invite for the given player
        /// </summary>
        /// <param name="Session">Player session</param>
        /// <returns></returns>
        public GroupInvite CreateInvite(WorldSession session)
        {
            var invite = new GroupInvite
            {
                Guid = session.Player.Guid,
                Session = session
            };
            Invites.Add(invite);
            return invite;
        }

        /// <summary>
        /// Find Invite for the given player, or null
        /// </summary>
        public GroupInvite FindInvite(WorldSession session)
        {
            return Invites.Find(invite => invite.Guid == session.Player.Guid);
        }

        /// <summary>
        /// Invite wasn't accepted or timed out
        /// </summary>
        public void DismissInvite(GroupInvite invite)
        {
            Invites.Remove(invite);
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        /// <param name="invite">member invite</param>
        public GroupMember AcceptInvite(GroupInvite invite)
        {
            Invites.Remove(invite);
            var member = new GroupMember
            {
                Id = GroupManager.NextGroupMemberId,
                Guid = invite.Guid,
                Session = invite.Session
            };
            Members.Add(member);
            return member;
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        /// <param name="session">member session</param>
        public GroupMember CreateMember(WorldSession session)
        {
            var member = new GroupMember
            {
                Id = GroupManager.NextGroupMemberId,
                Guid = session.Player.Guid,
                Session = session
            };
            Members.Add(member);
            return member;
        }

        public GroupMember FindMember(WorldSession session) => Members.Find(m => m.Guid == session.Player.Guid);

        public void RemoveMember(GroupMember member)
        {
            Members.Remove(member);
        }
    }
}
