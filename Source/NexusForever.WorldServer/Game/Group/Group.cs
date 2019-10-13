using NexusForever.Shared;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NexusForever.WorldServer.Game.Group
{
    public partial class Group: IUpdate
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
        /// Interval at which check to clear the invites
        /// TODO: move this to the config file
        /// </summary>
        private const double ClearInvitesInterval = 1d;

        /// <summary>
        /// Generate next unique group member ID
        /// </summary>
        private static ushort NextGroupMemberId => (ushort)Interlocked.Increment(ref nextGroupMemberId);
        private static int nextGroupMemberId = 1;

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
        /// Current party leader
        /// </summary>
        public GroupMember PartyLeader { get; private set; }

        /// <summary>
        /// Is this open world (non instance) group
        /// </summary>
        public bool IsOpenWorld => (Flags & GroupFlags.OpenWorld) != 0;

        /// <summary>
        /// Is this a raid group?
        /// </summary>
        public bool IsRaid => (Flags & GroupFlags.Raid) != 0;

        /// <summary>
        /// Max size for this group type
        /// </summary>
        public uint MaxSize => IsRaid ? MaxRaidSize : MaxPartySize;

        /// <summary>
        /// Group flags that can be sent to the client
        /// </summary>
        public GroupFlags Flags { get; private set; }

        /// <summary>
        /// Group can be dismissed if it has no members aside from group leader
        /// and no pending invites
        /// </summary>
        private bool ShouldDisband => (members.Count == 0) || (IsEmpty && invites.Count == 0);

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
        private readonly ReaderWriterLockSlim invitesLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Used for throttling the cleanup rate
        /// </summary>
        private double timeToClearInvites = ClearInvitesInterval;
        
        /// <summary>
        /// Create new Group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player">Initial party leader</param>
        public Group(ulong id, Player player)
        {
            Id = id;
            PartyLeader = CreateMember(player);
            Flags |= GroupFlags.OpenWorld;
            isNewGroup = true;
        }

        /// <summary>
        /// Clear out pending invites that have expired
        /// </summary>
        public void Update(double lastTick)
        {
            timeToClearInvites -= lastTick;
            if (timeToClearInvites <= 0d)
            {
                timeToClearInvites = ClearInvitesInterval;

                var now = DateTime.UtcNow;
                while (TryPeekInvite(out var invite))
                {
                    if (invite.ExpirationTime <= now)
                        ExpireInvite(invite);
                    else
                        break;
                }
            }
        }

        /// <summary>
        /// Get the oldest (first in the list) invite without removing it
        /// From the list.
        /// </summary>
        /// <returns>true if invite is found</returns>
        private bool TryPeekInvite(out GroupInvite invite)
        {
            invitesLock.EnterReadLock();
            var hasInvites = invites.Count > 0;
            invite = hasInvites ? invites[0] : null;
            invitesLock.ExitReadLock();
            return hasInvites;
        }

        /// <summary>
        /// Create invite for the given player
        /// </summary>
        /// <param name="inviter">group member who is inviting</param>
        /// <param name="invitee">player being invited</param>
        private GroupInvite CreateInvite(GroupMember inviter, Player invitee)
        {
            var invite = new GroupInvite(this, invitee, inviter);
            invitesLock.EnterWriteLock();
            invites.Add(invite);
            invitesLock.ExitWriteLock();
            invitee.GroupInvite = invite;
            return invite;
        }

        /// <summary>
        /// Invite wasn't accepted or timed out
        /// </summary>
        private void RemoveInvite(GroupInvite invite)
        {
            invite.Player.GroupInvite = null;
            invitesLock.EnterWriteLock();
            invites.Remove(invite);
            invitesLock.ExitWriteLock();
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        private GroupMember CreateMember(Player player)
        {
            var member = new GroupMember(NextGroupMemberId, this, player);
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

        /// <summary>
        /// Give next member in the group as candidate for the PartyLeader
        /// </summary>
        private GroupMember GetNextPartyLeader()
        {
            if (PartyLeader == null)
            {
                return members[0];
            }
            return members.Find(member => member.Id != PartyLeader.Id);
        }
    }
}
