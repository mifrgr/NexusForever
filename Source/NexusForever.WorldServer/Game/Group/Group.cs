using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    public class Group
    {
        public class Invite
        {
            public ulong CharacterId;
        }

        public class Member
        {
            public ulong Id;
            public ulong CharacterId;
        }

        public readonly ulong GroupId;

        public ulong PartyLeaderCharacterId = 0;

        private List<Invite> invites = new List<Invite>();

        private List<Member> members = new List<Member>();

        public bool IsEmpty => members.Count == 0;

        public Member PartyLeader => members.Find(m => m.CharacterId == PartyLeaderCharacterId);

        public Group(ulong groupId)
        {
            this.GroupId = groupId;
        }

        public Invite CreateNewInvite(ulong characterId)
        {
            var invite = new Invite
            {
                CharacterId = characterId
            };
            invites.Add(invite);
            return invite;
        }

        public Invite FindInvite(ulong characterId)
        {
            return invites.Find(i => i.CharacterId == characterId);
        }

        public Member AcceptInvite(Invite invite)
        {
            invites.Remove(invite);
            return CreateNewMember(invite.CharacterId);
        }

        public void DismissInvite(Invite invite)
        {
            invites.Remove(invite);
        }

        public Member CreateNewMember(ulong characterId)
        {
            var member = new Member
            {
                Id = GroupManager.NextGroupMemberId,
                CharacterId = characterId
            };

            members.Add(member);
            return member;
        }

        public Member FindMemberByGroupMemberId(ulong id)
        {
            return members.Find(m => m.Id == id);
        }

        public Member FindMemberByCharacterId(ulong id)
        {
            return members.Find(m => m.CharacterId == id);
        }

        public void RemoveMember(Member member)
        {
            members.Remove(member);
        }
    }
}
