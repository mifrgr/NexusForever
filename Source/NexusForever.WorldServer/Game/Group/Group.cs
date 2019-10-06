using NexusForever.WorldServer.Network;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group
    {
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
        public readonly ulong Id;

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

        public Group(ulong groupId) => Id = groupId;

        public Member CreateNewMember(WorldSession playerSession)
        {
            if (!GroupManager.GroupMember.ContainsKey(playerSession.Player.Guid))
            {
                var member = new Member
                {
                    Id = GroupManager.NextGroupMemberId,
                    Guid = playerSession.Player.Guid,
                    Session = playerSession
                };

                Members.Add(member);
                GroupManager.GroupMember.Add(playerSession.Player.Guid, member);

                return member;
            }
            else
                return GroupManager.GroupMember[playerSession.Player.Guid];
        }

        public Member FindMember(WorldSession session) => Members.Find(m => m.Guid == session.Player.Guid);

        public void RemoveMember(Member member)
        {
            GroupManager.GroupMember.Remove(member.Guid);
            Members.Remove(member);
        }
    }
}
