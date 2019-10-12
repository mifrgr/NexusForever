using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Game.Group
{
    public partial class Group
    {
        /// <summary>
        /// Unique group member ID
        /// </summary>
        private static ushort nextGroupMemberId;

        static Group()
        {
            nextGroupMemberId = 1;
        }

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
        public bool IsEmpty => members.Count <= 1;

        /// <summary>
        /// True if max number of players in the group.
        /// </summary>
        public bool IsFull => members.Count == MaxSize;

        /// <summary>
        /// Group can be dismissed if it has no members aside from group leader
        /// and no pending invites
        /// </summary>
        public bool ShouldDisband => (members.Count == 0) || (IsEmpty && invites.Count == 0);

        /// <summary>
        /// True if this group has pending invites
        /// </summary>
        public bool HasPendingInvites => invites.Count > 0;

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
                    return members[0];
                }
                return members.Find(member => member.Id != PartyLeader.Id);
            }
        }

        /// <summary>
        /// Is this open world (non instance) group
        /// </summary>
        public bool IsOpenWorld
        {
            get { return (Flags & GroupFlags.OpenWorld) != 0; }
            private set
            {
                if (value)
                    Flags |= GroupFlags.OpenWorld;
                else
                    Flags &= ~GroupFlags.OpenWorld;
            }
        }

        /// <summary>
        /// Is this a raid group?
        /// </summary>
        public bool IsRaid
        {
            get { return (Flags & GroupFlags.Raid) != 0; }
            private set
            {
                if (value)
                    Flags |= GroupFlags.Raid;
                else
                    Flags &= ~GroupFlags.Raid;
            }
        }

        /// <summary>
        /// Max size for this group type
        /// </summary>
        public uint MaxSize => IsRaid ? MaxRaidSize : MaxPartySize;

        /// <summary>
        /// Group flags that can be sent to the client
        /// </summary>
        public GroupFlags Flags { get; private set; }

        /// <summary>
        /// Group is new if member info has not been sent to the client yet
        /// </summary>
        private bool isNewGroup;

        /// <summary>
        /// Logger for the groups
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Group members for private usage
        /// </summary>
        private readonly List<GroupMember> members = new List<GroupMember>();

        /// <summary>
        /// Players who have been invited or who have request to join the group
        /// </summary>
        private readonly List<GroupInvite> invites = new List<GroupInvite>();

        /// <summary>
        /// Create new Group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player">Initial party leader</param>
        public Group(ulong id, Player player)
        {
            Id = id;
            PartyLeader = CreateMember(player);
            IsOpenWorld = true;
            isNewGroup = true;
        }

        /// <summary>
        /// Clear out pending invites that have expired
        /// </summary>
        public void ClearExpiredInvites(DateTime now)
        {
            while (HasPendingInvites)
            {
                var invite = invites[0];
                if (invite.ExpirationTime <= now)
                    ExpireInvite(invite);
                else
                    return;
            }
        }

        /// <summary>
        /// Create invite for the given player
        /// </summary>
        /// <param name="inviter">group member who is inviting</param>
        /// <param name="invitee">player being invited</param>
        private GroupInvite CreateInvite(GroupMember inviter, Player invitee)
        {
            var invite = new GroupInvite(this, invitee, inviter);
            invites.Add(invite);
            invitee.GroupInvite = invite;
            return invite;
        }

        /// <summary>
        /// Invite wasn't accepted or timed out
        /// </summary>
        private void RemoveInvite(GroupInvite invite)
        {
            invite.Player.GroupInvite = null;
            invites.Remove(invite);
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        private GroupMember CreateMember(Player player)
        {
            var member = new GroupMember(nextGroupMemberId++, this, player);
            members.Add(member);
            player.GroupMember = member;
            return member;
        }

        /// <summary>
        /// Remove member from the group
        /// </summary>
        private void RemoveMember(GroupMember member)
        {
            members.Remove(member);
            member.Player.GroupMember = null;
            if (PartyLeader?.Id == member.Id)
                PartyLeader = null;
        }

        /// <summary>
        /// Find member in the group
        /// </summary>
        private GroupMember FindMember(TargetPlayerIdentity target)
        {
            return members.Find(m => m.Player.CharacterId == target.CharacterId);
        }
    }
}
