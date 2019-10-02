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
            public ulong CharacterId;
        }

        public readonly ulong GroupId;

        public ulong PartyLeaderCharacterId = 0;

        private List<Invite> invites = new List<Invite>();

        private List<Member> members = new List<Member>();

        public bool IsEmpty => members.Count == 0;

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
            var member = new Member
            {
                CharacterId = invite.CharacterId
            };

            members.Add(member);
            invites.Remove(invite);

            return member;
        }

        public void DismissInvite(Invite invite)
        {
            invites.Remove(invite);
        }
    }
}
