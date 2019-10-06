using NexusForever.WorldServer.Network;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group
    {
        /// <summary>
        /// Represent player that has been invited to the group,
        /// but is not yet a member. Waiting for accept.
        /// 
        /// Players can either be invited, or request to join (TODO)
        /// </summary>
        public class Invite
        {
            public ulong Guid;
            public WorldSession Session;

            /// <summary>
            /// Member who invited
            /// </summary>
            public Member Inviter;
        }

        /// <summary>
        /// Represent player that is part of the group
        /// </summary>
        public class Member
        {
            public ulong Id;
            public ulong Guid;
            public WorldSession Session;
        }

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
        public Member PartyLeader;

        /// <summary>
        /// Group members
        /// </summary>
        public List<Member> Members { get; } = new List<Member>();

        /// <summary>
        /// Players who have been invited or who have request to join the group
        /// </summary>
        public List<Invite> Invites { get; } = new List<Invite>();

        /// <summary>
        /// Create invite for the given player
        /// </summary>
        /// <param name="Session">Player session</param>
        /// <returns></returns>
        public Invite CreateInvite(WorldSession session)
        {
            var invite = new Invite
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
        public Invite FindInvite(WorldSession session)
        {
            return Invites.Find(invite => invite.Guid == session.Player.Guid);
        }

        /// <summary>
        /// Invite wasn't accepted or timed out
        /// </summary>
        public void DismissInvite(Invite invite)
        {
            Invites.Remove(invite);
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        /// <param name="invite">member invite</param>
        public Member AcceptInvite(Invite invite)
        {
            Invites.Remove(invite);
            var member = new Member
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
        public Member CreateMember(WorldSession session)
        {
            var member = new Member
            {
                Id = GroupManager.NextGroupMemberId,
                Guid = session.Player.Guid,
                Session = session
            };
            Members.Add(member);
            return member;
        }

        public Member FindMember(WorldSession session) => Members.Find(m => m.Guid == session.Player.Guid);

        public void RemoveMember(Member member)
        {
            Members.Remove(member);
        }
    }
}
