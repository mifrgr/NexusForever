using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Collections.Generic;

namespace NexusForever.WorldServer.Game.Group
{
    public partial class Group
    {
        /// <summary>
        /// Maxoimum size party (non raid) group can be
        /// </summary>
        public const uint MaxPartySize = 5;

        /// <summary>
        /// Maximum size raid group can be
        /// </summary>
        public const uint MaxRaidSize = 20;

        /// <summary>
        /// Unique Group ID
        /// </summary>
        public readonly ulong Id;

        /// <summary>
        /// True of group has no other members aside from party leader in it and
        /// </summary>
        public bool IsEmpty => Members.Count <= 1;

        /// <summary>
        /// Group can be dismissed if it has no members aside from group leader
        /// and no pending invites
        /// </summary>
        public bool CanDismiss => !IsEmpty && Invites.Count == 0;

        /// <summary>
        /// Current party leader
        /// </summary>
        public GroupMember PartyLeader { get; private set; }

        /// <summary>
        /// Give next member in the group as candidate for the PartyLeader
        /// </summary>
        public GroupMember NextPartyLeaderCandidate {
            get
            {
                if (PartyLeader == null)
                {
                    return Members[0];
                }
                return Members.Find(member => member.Id != PartyLeader.Id);
            }
        }

        /// <summary>
        /// Group members
        /// </summary>
        public List<GroupMember> Members { get; } = new List<GroupMember>();

        /// <summary>
        /// Players who have been invited or who have request to join the group
        /// </summary>
        public List<GroupInvite> Invites { get; } = new List<GroupInvite>();

        /// <summary>
        /// Is this open world (non instance) group
        /// </summary>
        public bool IsOpenWorld
        {
            get { return (Flags & GroupTypeFlags.OpenWorld) != 0; }
            set
            {
                if (value)
                    Flags |= GroupTypeFlags.OpenWorld;
                else
                    Flags &= ~GroupTypeFlags.OpenWorld;
            }
        }

        /// <summary>
        /// Is this an instance group?
        /// </summary>
        public bool IsInstance
        {
            get { return !IsOpenWorld; }
            set { IsOpenWorld = !value;  }
        }

        /// <summary>
        /// Is this a raid group?
        /// </summary>
        public bool IsRaid
        {
            get { return (Flags & GroupTypeFlags.Raid) != 0; }
            set
            {
                if (value)
                    Flags |= GroupTypeFlags.Raid;
                else
                    Flags &= ~GroupTypeFlags.Raid;
            }
        }

        /// <summary>
        /// Is this a normal party group
        /// </summary>
        public bool IsParty
        {
            get { return !IsRaid; }
            set { IsRaid = !value; }
        }

        /// <summary>
        /// Max size for this group type
        /// </summary>
        public uint MaxSize => IsParty ? MaxPartySize : MaxRaidSize;

        /// <summary>
        /// Group flags that can be sent to the client
        /// </summary>
        public GroupTypeFlags Flags { get; private set; }

        /// <summary>
        /// Create new Group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player">Initial party leader</param>
        public Group(ulong id, Player player)
        {
            Id = id;
            SetPartyLeader(CreateMember(player));
            IsOpenWorld = true;
        }

        /// <summary>
        /// Create invite for the given player
        /// </summary>
        /// <param name="member">group member who is inviting</param>
        /// <param name="player">player being invited</param>
        public GroupInvite CreateInvite(GroupMember member, Player player)
        {
            var invite = new GroupInvite
            {
                Group = this,
                Player = player,
                Inviter = member
            };
            Invites.Add(invite);
            player.GroupInvite = invite;
            return invite;
        }

        /// <summary>
        /// Invite wasn't accepted or timed out
        /// </summary>
        public void DismissInvite(GroupInvite invite)
        {
            invite.Player.GroupInvite = null;
            Invites.Remove(invite);
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        public GroupMember AcceptInvite(GroupInvite invite)
        {
            DismissInvite(invite);
            return CreateMember(invite.Player);
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        public GroupMember CreateMember(Player player)
        {
            var member = new GroupMember
            {
                Id = GlobalGroupManager.NextGroupMemberId,
                Group = this,
                Player = player
            };
            member.SetIsPartyLeader(false);
            Members.Add(member);
            player.GroupMember = member;
            return member;
        }

        /// <summary>
        /// Remove member from the group
        /// </summary>
        public void RemoveMember(GroupMember member)
        {
            Members.Remove(member);
            member.Player.GroupMember = null;
            if (PartyLeader?.Id == member.Id)
            {
                SetPartyLeader(null);
            }
        }

        /// <summary>
        /// Find member in the group
        /// </summary>
        public GroupMember FindMember(TargetPlayerIdentity target)
        {
            return Members.Find(m => m.Player.CharacterId == target.CharacterId);
        }

        /// <summary>
        /// Set member as party leader
        /// </summary>
        public void SetPartyLeader(GroupMember member)
        {
            if (PartyLeader != null)
                PartyLeader.SetIsPartyLeader(false);

            PartyLeader = member;

            if (PartyLeader != null)
                PartyLeader.SetIsPartyLeader(true);
        }
    }
}
