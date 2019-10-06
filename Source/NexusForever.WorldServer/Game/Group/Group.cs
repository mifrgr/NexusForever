using NexusForever.WorldServer.Network;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group
    {
        public class Member
        {
            public ulong Id;
            public ulong Guid;
            public WorldSession PlayerSession;
        }

        public bool IsEmpty => Members.Count == 0;
        public readonly ulong GroupId;
        public ulong PartyLeadGuid = 0;
        public Member PartyLeader => Members.Find(m => m.Guid == PartyLeadGuid);

        public List<Member> Members { get; } = new List<Member>();

        public Group(ulong groupId) => GroupId = groupId;

        public Member CreateNewMember(WorldSession playerSession)
        {
            if (!GroupManager.GroupMember.ContainsKey(playerSession.Player.Guid))
            {
                var member = new Member
                {
                    Id = GroupManager.NextGroupMemberId,
                    Guid = playerSession.Player.Guid,
                    PlayerSession = playerSession
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
