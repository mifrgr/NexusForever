using NexusForever.Shared;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Extensions;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    public sealed partial class Group : IUpdate
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
        /// Interval at which group member positions update is sent
        /// </summary>
        private const double UpdatePositionsInterval = 1.0;

        /// <summary>
        /// Generate next unique group member ID
        /// </summary>
        private static ushort NextGroupMemberId => (ushort)Interlocked.Increment(ref nextGroupMemberId);
        private static int nextGroupMemberId = 1;

        /// <summary>
        /// Logger for the groups
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Unique Group ID
        /// </summary>
        public readonly ulong Id;

        /// <summary>
        /// True if max number of players in the group.
        /// </summary>
        public bool IsFull => members.Count == MaxSize;

        /// <summary>
        /// Current party leader
        /// </summary>
        public GroupMember PartyLeader { get; private set; }

        /// <summary>
        /// Group flags. Use Flags accessor to get correct flags
        /// </summary>
        public GroupFlags Flags { get; private set; }

        /// <summary>
        /// Group join requests setting
        /// </summary>
        public InvitationMethod JoinRequestsMethod
        {
            get
            {
                if ((Flags & GroupFlags.JoinRequestOpen) != 0)
                    return InvitationMethod.Open;

                if ((Flags & GroupFlags.JoinRequestClosed) != 0)
                    return InvitationMethod.Closed;

                return InvitationMethod.Neutral;
            }
        }

        /// <summary>
        /// Group referrals settings
        /// </summary>
        public InvitationMethod ReferralsMethod
        {
            get
            {
                if ((Flags & GroupFlags.ReferralsOpen) != 0)
                    return InvitationMethod.Open;

                if ((Flags & GroupFlags.ReferralsClosed) != 0)
                    return InvitationMethod.Closed;

                return InvitationMethod.Neutral;
            }
        }

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
        /// Group can be dismissed if it has no members aside from group leader
        /// and no pending invites
        /// </summary>
        private bool ShouldDisband => (members.Count == 0) || (members.Count == 1 && invites.Count == 0);

        /// <summary>
        /// Group is new if member info has not been sent to the client yet
        /// </summary>
        private bool isNewGroup;

        /// <summary>
        /// Group members.
        /// </summary>
        private readonly List<GroupMember> members = new List<GroupMember>();
        private readonly ReaderWriterLockSlim membersLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Players who have been invited or who have request to join the group
        /// </summary>
        private readonly List<GroupInvite> invites = new List<GroupInvite>();
        private readonly ReaderWriterLockSlim invitesLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Pending referrals that are sent to the party leader for confirmation
        /// </summary>
        private readonly List<GroupInvite> referrals = new List<GroupInvite>();

        /// <summary>
        /// Used for throttling the cleanup rate
        /// </summary>
        private double timeToClearInvites = ClearInvitesInterval;

        /// <summary>
        /// Used for throttling group position updates
        /// </summary>
        private double timeToUpdatePositions = UpdatePositionsInterval;

        /// <summary>
        /// Create new Group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player">Initial party leader</param>
        public Group(ulong id, Player player)
        {
            Id = id;
            PartyLeader = CreateMember(player);
            SetFlags(GroupFlags.OpenWorld);
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

            timeToUpdatePositions -= lastTick;
            if (timeToUpdatePositions <= 0d)
            {
                timeToUpdatePositions = UpdatePositionsInterval;
                UpdatePositions();
            }
        }

        private void UpdatePositions()
        {
            //using (membersLock.GetReadLock())
            //{
            //    foreach (var member in members)
            //    {
            //        if (member.ZoneId != member.Player.Zone.Id)
            //        {
            //            member.ZoneId = (ushort)member.Player.Zone.Id;
            //            members.ForEach( m => m.Send(new ServerGroupUpdatePlayerRealm
            //            {
            //                GroupId = Id,
            //                TargetPlayer = member.Player.BuildTargetPlayerIdentity(),
            //                Realm = WorldServer.RealmId,
            //                WorldZoneId = (ushort)member.ZoneId,
            //                Unknown25 = member.Player.Map.Entry.Id,
            //                Unknown26 = 1,
            //                SyncedToGroup = true
            //            }));
            //        }
            //    }

            //    var updates = new Dictionary<ushort, ServerGroupPositionUpdate>();
            //    foreach (var member in members)
            //    {
            //        if (!updates.TryGetValue(member.ZoneId, out var update))
            //        {
            //            update = new ServerGroupPositionUpdate
            //            {
            //                GroupId = Id,
            //                WorldZoneId = member.ZoneId,
            //                Entries = new List<ServerGroupPositionUpdate.Entry>()
            //            };
            //            updates.Add(member.ZoneId, update);
            //        }

            //        var player = member.Player;
            //        var entry = new ServerGroupPositionUpdate.Entry
            //        {
            //            Player = player.BuildTargetPlayerIdentity(),
            //            Position = new Position(player.Position),
            //            UnitId = member.Player.Guid,
            //            Flags = 0
            //        };
            //        update.Entries.Add(entry);
            //    }

            //    foreach (var item in updates)
            //    {
            //        members.ForEach(m => m.Send(item.Value));
            //    }
            //}
        }

        /// <summary>
        /// Get the oldest (first in the list) invite without removing it
        /// From the list.
        /// </summary>
        /// <returns>true if invite is found</returns>
        private bool TryPeekInvite(out GroupInvite invite)
        {
            using (invitesLock.GetReadLock())
            {
                var hasInvite = invites.Count > 0;
                #nullable disable
                invite = hasInvite ? invites[0] : null;
                #nullable enable
                return hasInvite;
            }
        }

        /// <summary>
        /// Create invite for the given player
        /// </summary>
        /// <param name="inviter">group member who is inviting</param>
        /// <param name="invitee">player being invited</param>
        private GroupInvite CreateInvite(GroupMember inviter, Player invitee, GroupInviteType type)
        {
            var invite = new GroupInvite(this, invitee, inviter, type);
            using (invitesLock.GetWriteLock())
            {
                invites.Add(invite);
                if (type == GroupInviteType.Referral)
                    referrals.Add(invite);
                else
                    invitee.GroupInvite = invite;
            }
            return invite;
        }

        /// <summary>
        /// Invite wasn't accepted or timed out
        /// </summary>
        private void RemoveInvite(GroupInvite invite)
        {
            using (invitesLock.GetWriteLock())
            {
                invites.Remove(invite);
                if (invite.Type == GroupInviteType.Referral)
                    referrals.Remove(invite);
                else
                    invite.Player.GroupInvite = null;
            }
        }

        /// <summary>
        /// Find invite referral for given player name
        /// </summary>
        public GroupInvite? FindReferral(string playerName)
        {
            using (invitesLock.GetReadLock())
            {
                return referrals.Find(i => i.Player.Name == playerName);
            }
        }

        /// <summary>
        /// Get copy of group members in a thread safe way
        /// </summary>
        public List<GroupMember> GetMembers()
        {
            using (membersLock.GetReadLock())
            {
                return members.ToList();
            }
        }

        /// <summary>
        /// Add new member to the group
        /// </summary>
        private GroupMember CreateMember(Player player)
        {
            var member = new GroupMember(NextGroupMemberId, this, player);
            using (membersLock.GetWriteLock())
            {
                members.Add(member);
                player.GroupMember = member;
            }
            return member;
        }

        /// <summary>
        /// Remove member from the group
        /// </summary>
        private void RemoveMember(GroupMember member)
        {
            using (membersLock.GetWriteLock())
            {
                members.Remove(member);
                member.Player.GroupMember = null;
            }
        }

        /// <summary>
        /// Find member in the group
        /// </summary>
        private GroupMember? FindMember(TargetPlayerIdentity target)
        {
            using (membersLock.GetReadLock())
            {
                return members.Find(m => m.Player.CharacterId == target.CharacterId);
            }
        }

        /// <summary>
        /// Give next member in the group as candidate for the PartyLeader
        /// </summary>
        private GroupMember GetNextPartyLeader()
        {
            using (membersLock.GetReadLock())
            {
                if (PartyLeader is null) return members[0];
                return members.Find(member => member.Id != PartyLeader.Id);
            }
        }

        /// <summary>
        /// Set group flags
        /// </summary>
        private void SetFlags(GroupFlags flags)
        {
            GroupFlags set = 0;

            // Raid
            if ((flags & GroupFlags.Raid) != 0)
            {
                set |= GroupFlags.Raid;
            }

            // Join request
            if ((flags & GroupFlags.JoinRequestOpen) != 0)
            {
                set |= GroupFlags.JoinRequestOpen;
            }
            else if ((flags & GroupFlags.JoinRequestClosed) != 0)
            {
                set |= GroupFlags.JoinRequestClosed;
            }

            // Referral
            if ((flags & GroupFlags.ReferralsOpen) != 0)
            {
                set |= GroupFlags.ReferralsOpen;
            }
            else if ((flags & GroupFlags.ReferralsClosed) != 0)
            {
                set |= GroupFlags.ReferralsClosed;
            }

            // open world
            if ((flags & GroupFlags.OpenWorld) != 0)
            {
                set |= GroupFlags.OpenWorld;
            }

            Flags = set;
        }
    }
}
