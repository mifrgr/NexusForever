using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using System.Linq;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    public partial class Group
    {
        /// <summary>
        /// Invite player to the group by name
        /// </summary>
        public void Invite(Player inviter, string playerName)
        {
            var member = ValidatePlayer(inviter);
            void sendReply(InviteResult result)
            {
                member.Send(BuildServerGroupInviteResult(playerName, result));
                if (ShouldDisband) Disband();
            }

            inviter.Session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterByName(playerName), character =>
            {
                if (character == null)
                {
                    sendReply(InviteResult.PlayerNotFound);
                    return;
                }
                
                var inviteeSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                if (inviteeSession == null)
                {
                    sendReply(InviteResult.PlayerNotFound);
                    return;
                }

                Invite(inviter, inviteeSession.Player);
            }));
        }

        /// <summary>
        /// Invite player to the group
        /// </summary>
        public void Invite(Player inviter, Player invitee)
        {
            var member = ValidatePlayer(inviter);
            void sendReply(InviteResult result)
            {
                member.Send(BuildServerGroupInviteResult(invitee.Name, result));
                if (ShouldDisband) Disband();
            }

            if (invitee.GroupMember != null)
            {
                sendReply(InviteResult.Grouped);
                return;
            }

            if (invitee.GroupInvite != null)
            {
                sendReply(InviteResult.IsInvited);
                return;
            }

            if (inviter.Guid == invitee.Guid)
            {
                sendReply(InviteResult.NoInvitingSelf);
                return;
            }

            if (!member.CanInvite)
            {
                sendReply(InviteResult.NoPermissions);
                return;
            }

            if (IsFull)
            {
                sendReply(InviteResult.Full);
                return;
            }

            // create invite and send responses
            var invite = CreateInvite(member, invitee, GroupInviteType.Invite);
            sendReply(InviteResult.Sent);
            invite.Send(BuildServerGroupInviteReceived());
        }

        /// <summary>
        /// Process invite response
        /// </summary>
        public void HandleInvite(GroupInvite invite, InviteResponseResult response)
        {
            ValidateInvite(invite);
            RemoveInvite(invite);

            if (response == InviteResponseResult.Declined)
            {
                invite.Inviter.Send(BuildServerGroupInviteResult(invite.Player.Name, InviteResult.Declined));
                if (ShouldDisband) Disband();
                return;
            }

            if (IsFull)
            {
                var isFull = BuildServerGroupInviteResult(invite.Player.Name, InviteResult.Full);
                invite.Send(isFull);
                invite.Inviter.Send(isFull);
                return;
            }

            // Accept
            invite.Inviter.Send(BuildServerGroupInviteResult(invite.Player.Name, InviteResult.Accepted));
            Add(invite.Player);
        }

        /// <summary>
        /// Handle expired invite and clear the state
        /// </summary>
        public void ExpireInvite(GroupInvite invite)
        {
            RemoveInvite(invite);
            invite.Inviter.Send(BuildServerGroupInviteResult(invite.Player.Name, InviteResult.ExpiredInviter));
            invite.Send(BuildServerGroupInviteResult(invite.Player.Name, InviteResult.ExpiredInvitee));
            if (ShouldDisband) Disband();
        }

        /// <summary>
        /// Player has requested to join the group
        /// </summary>
        public void RequestJoin(Player player)
        {
            void sendJoinResult(InviteResult result)
            {
                player.Session.EnqueueMessageEncrypted(BuildServerGroupRequestJoinResult("", result, true));
            }

            if (player.GroupInvite != null)
            {
                return;
            }

            if (player.GroupMember != null)
            {
                sendJoinResult(InviteResult.Grouped);
                return;
            }

            if ((Flags & GroupFlags.JoinRequestClosed) != 0)
            {
                sendJoinResult(InviteResult.NotAcceptingRequests);
                return;
            }

            if (IsFull)
            {
                sendJoinResult(InviteResult.Full);
                return;
            }

            if ((Flags & GroupFlags.JoinRequestOpen) != 0)
            {
                Add(player);
                return;
            }

            var invite = CreateInvite(PartyLeader, player, GroupInviteType.Requst);
            invite.Send(BuildServerGroupRequestJoinResult(PartyLeader.Player.Name, InviteResult.Sent, true));

            PartyLeader.Send(BuildServerGroupRequestJoin(invite));
        }

        /// <summary>
        /// Handle request to join response
        /// </summary>
        /// <param name="invite">invite of the player who requested to join</param>
        /// <param name="accepted">party leader response</param>
        public void HandleRequestJoin(GroupInvite invite, bool accepted)
        {
            ValidateInvite(invite);
            RemoveInvite(invite);

            if (!accepted)
            {
                invite.Send(BuildServerGroupRequestJoinResult(invite.Inviter.Player.Name, InviteResult.Declined, true));
                return;
            }

            if (IsFull)
            {
                invite.Send(BuildServerGroupRequestJoinResult("", InviteResult.Full, true));
                return;
            }

            // Accept
            invite.Send(BuildServerGroupRequestJoinResult(invite.Inviter.Player.Name, InviteResult.Accepted, true));
            Add(invite.Player);
        }

        /// <summary>
        /// Player is leaving the group
        /// </summary>
        /// <param name="scope">Party leader can also disband</param>
        public void Leave(Player player, GroupLeaveScope scope)
        {
            var member = ValidatePlayer(player);
            
            if (scope == GroupLeaveScope.Disband && member.IsPartyLeader)
            {
                Disband();
                return;
            }

            Remove(player, RemoveReason.Left);
        }

        /// <summary>
        /// Promote another player to party lead
        /// </summary>
        public void Promote(Player player, TargetPlayerIdentity identity)
        {
            var member = ValidatePlayer(player);

            if (!member.IsPartyLeader)
                return;

            var newLeader = FindMember(identity);
            if (newLeader == null)
                return;

            Promote(newLeader.Player);
        }

        /// <summary>
        /// Promote given member to group lead
        /// </summary>
        public void Promote(Player player)
        {
            var member = ValidatePlayer(player);

            var oldLeader = PartyLeader;
            PartyLeader = member;

            Broadcast(member.BuildServerGroupPromote());

            if (oldLeader != null)
                Broadcast(oldLeader.BuildServerGroupMemberFlagsChanged(true));

            Broadcast(member.BuildServerGroupMemberFlagsChanged(true));
        }

        /// <summary>
        /// Add specified Player to the group
        /// </summary>
        public void Add(Player player)
        {
            if (IsFull || player.GroupMember != null || player.GroupInvite != null)
                return;

            var newMember = CreateMember(player);

            if (isNewGroup)
            {
                isNewGroup = false;
                Broadcast(member => BuildServerGroupJoin(member));
            }
            else
            {
                newMember.Send(BuildServerGroupJoin(newMember));
                var addMember = BuildServerGroupMemberAdd(newMember);
                Broadcast(addMember, newMember);
            }

            if (IsFull)
            {
                while (TryPeekInvite(out var invite))
                    ExpireInvite(invite);
            }
        }

        /// <summary>
        /// Kick given player from the group
        /// </summary>
        /// <param name="player">player who initiated the kick</param>
        /// <param name="identity">target player to remove</param>
        public void Kick(Player player, TargetPlayerIdentity identity)
        {
            var member = ValidatePlayer(player);

            if (!member.CanKick)
                return;

            var kickedMember = FindMember(identity);
            if (kickedMember == null)
                return;

            if (kickedMember.IsPartyLeader)
                return;

            if (kickedMember.Id == member.Id)
                return;

            Remove(kickedMember.Player, RemoveReason.Kicked);
        }

        /// <summary>
        /// Remove given player from the group
        /// </summary>
        public void Remove(Player player, RemoveReason reason)
        {
            var member = ValidatePlayer(player);

            RemoveMember(member);
            member.Send(BuildServerGroupLeave(reason));
            Broadcast(member.BuildServerGroupRemove(reason));

            if (ShouldDisband)
            {
                Disband();
                return;
            }

            if (PartyLeader == null)
            {
                Promote(GetNextPartyLeader().Player);
            }
        }

        /// <summary>
        /// Update target player flags
        /// </summary>
        /// <param name="changed">flags that have changed</param>
        /// <param name="flags">new flag values</param>
        public void UpdateFlags(Player player, TargetPlayerIdentity identity, GroupMemberInfoFlags changed, GroupMemberInfoFlags flags)
        {
            var member = ValidatePlayer(player);

            var targetMember = FindMember(identity);
            if (targetMember == null)
                return;

            if (!member.CanUpdateFlags(changed, targetMember))
                return;

            UpdateFlags(targetMember.Player, changed, flags);
        }

        /// <summary>
        /// Update group member flags
        /// </summary>
        public void UpdateFlags(Player player, GroupMemberInfoFlags changed, GroupMemberInfoFlags flags)
        {
            var member = ValidatePlayer(player);

            // TODO: encapsulate
            var set = flags & changed;
            member.SetFlags(set, true);

            var unset = ~flags & changed;
            member.SetFlags(unset, false);

            Broadcast(member.BuildServerGroupMemberFlagsChanged(false));
        }

        /// <summary>
        /// Update group flags
        /// </summary>
        /// <param name="player">player doing the flag change</param>
        /// <param name="flags">flag that is being changed</param>
        public void UpdateFlags(Player player, GroupFlags flags)
        {
            var member = ValidatePlayer(player);
            if (!member.IsPartyLeader)
                return;

            UpdateFlags(flags);
        }

        /// <summary>
        /// Change flags of group
        /// </summary>
        /// <param name="flags">flag that is being changed</param>
        public void UpdateFlags(GroupFlags flags)
        {
            var wasRaidAlready = IsRaid;

            if ((flags & (GroupFlags.Raid)) != 0)
                Flags = GroupFlags.Raid;
            else
                Flags = flags;

            Broadcast(BuildServerGroupFlagsChanged());

            if (!wasRaidAlready && IsRaid)
                Broadcast(BuildServerGroupMaxSizeChange());
        }

        /// <summary>
        /// Initiate ready check from the given player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="message"></param>
        public void ReadyCheck(Player player, string message)
        {
            var member = ValidatePlayer(player);

            if (!member.CanReadyCheck)
                return;

            Broadcast(member =>
            {
                member.PrepareForReadyCheck();
                return member.BuildServerGroupMemberFlagsChanged(false);
            });

            Broadcast(BuildServerGroupSendReadyCheck(member, message));
        }

        /// <summary>
        /// This will dismiss the group. All ivnites will be cancelled
        /// and members will be sent group disband notice
        /// </summary>
        public void Disband()
        {
            var serverLeave = BuildServerGroupLeave(RemoveReason.Disband);
            Members.ForEach(member =>
            {
                member.Send(serverLeave);
                RemoveMember(member);
            });

            while (TryPeekInvite(out var invite))
            { 
                invite.Send(BuildServerGroupInviteResult(invite.Player.Name, InviteResult.ExpiredInvitee));
                RemoveInvite(invite);
            };

            GlobalGroupManager.RemoveGroup(this);
        }

        #region Teleport

        /// <summary>
        /// Teleport group to the given player's location
        /// </summary>
        public void Teleport(Player player)
        {
            var worldId = (ushort)player.Map.Entry.Id;
            var x = player.Position.X;
            var y = player.Position.Y;
            var z = player.Position.Z;

            Members.ForEach(member =>
            {
               if (member.Player.Guid == player.Guid)
                   return;
               member.Player.TeleportTo(worldId, x, y, z);
            });
        }

        /// <summary>
        /// Teleport group to the given coordinates
        /// </summary>
        public void Teleport(ushort worldId, float x, float y, float z)
        {
            Members.ForEach(member =>
            {
                member.Player.TeleportTo(worldId, x, y, z);
            });
        }

        #endregion

        #region Validators

        /// <summary>
        /// Validates that given member is party of this group
        /// </summary>
        /// <returns>member object</returns>
        private GroupMember ValidatePlayer(Player player)
        {
            if (player.GroupMember?.Group.Id != Id)
                throw new InvalidPacketValueException();

            return player.GroupMember;
        }

        /// <summary>
        /// Validate that invite is valid for this group
        /// </summary>
        private void ValidateInvite(GroupInvite invite)
        {
            if (invite.Group.Id != Id)
                throw new InvalidPacketValueException();
        }

        #endregion
    }
}
