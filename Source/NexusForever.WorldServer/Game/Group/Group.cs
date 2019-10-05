using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
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
        public ulong PartyLead = 0;
        public Member PartyLeader => Members.Find(m => m.Guid == PartyLead);

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

        public Member FindMemberByMemberId(ulong id) => Members.Find(m => m.Id == id);

        public Member FindMemberByPlayerGuid(ulong guid) => Members.Find(m => m.Guid == guid);

        public void RemoveMember(Member member) => Members.Remove(member);
    }
}
