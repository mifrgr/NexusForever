using NexusForever.Shared.Game.Events;
using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Database.Character;
using NexusForever.WorldServer.Database.Character.Model;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using NLog;
using System.Collections.Generic;
using System.Linq;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();


        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite request)
        {
            session.EnqueueEvent(new TaskGenericEvent<Character>(CharacterDatabase.GetCharacterByName(request.PlayerName), character =>
            {
                void sendGroupInviteResult(InviteResult result, ulong groupId = 0)
                {
                    session.EnqueueMessageEncrypted(new ServerGroupInviteResult
                    {
                        GroupId = groupId,
                        PlayerName = request.PlayerName,
                        Result = result
                    });
                }

                log.Info($"{session.Player.Name} has invited {request.PlayerName} to group");

                // Invalid character?
                if (character == null)
                {
                    sendGroupInviteResult(InviteResult.PlayerNotFound);
                    return;
                }

                // Player not online?
                var targetSession = NetworkManager<WorldSession>.GetSession(s => s.Player?.CharacterId == character.Id);
                if (targetSession == null)
                {
                    sendGroupInviteResult(InviteResult.PlayerNotFound);
                    return;
                }

                var player = session.Player;
                var targetPlayer = targetSession.Player;

                // Player already in the group?
                if (targetPlayer.GroupMember != null)
                {
                    sendGroupInviteResult(InviteResult.PlayerAlreadyInGroup);
                    return;
                }

                // Player already has a pending invite
                if (targetPlayer.GroupInvite != null)
                {
                    sendGroupInviteResult(InviteResult.PlayerAlreadyInvited);
                    return;
                }

                // Inviting yourself?
                if (player.Guid == targetPlayer.Guid)
                {
                    sendGroupInviteResult(InviteResult.CannotInviteYourself);
                    return;
                }

                // are we creating a new group, or inviting into
                // existing one?
                var group = player.GroupMember?.Group;
                if (group == null)
                {
                    log.Info($"Creating a new group");
                    group = GlobalGroupManager.CreateGroup(player);
                    group.IsRaid = true;
                }
                // Trying to invite without permission!
                else if (!player.GroupMember.CanInvite)
                {
                    sendGroupInviteResult(InviteResult.NotPermitted);
                    return;
                }

                log.Info($"Creating invite");
                group.CreateInvite(player.GroupMember, targetPlayer);
                
                sendGroupInviteResult(InviteResult.Sent, group.Id);

                var groupMembers = new List<Member>();
                foreach (var member in group.Members)
                {
                    groupMembers.Add(new Member
                    {
                        Name = member.Player.Name,
                        Faction = member.Player.Faction1,
                        Race = member.Player.Race,
                        Class = member.Player.Class,
                        Sex = member.Player.Sex,
                        Path = member.Player.Path,
                        Level = (byte)member.Player.Level,
                        GroupMemberId = member.Id
                    });
                }
                targetSession.EnqueueMessageEncrypted(new ServerGroupInviteReceived
                {
                    GroupId = group.Id,
                    GroupMembers = groupMembers
                });
            }));
        }
        
        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse request)
        {
            log.Info($"{session.Player.Name} has responded to group#{request.GroupId} invite with {request.Response}");

            var player = session.Player;

            var invite = player.GroupInvite;
            if (invite == null)
                return;

            var group = invite.Group;
            if (group.Id != request.GroupId)
                return;

            // Declined
            if (request.Response == InviteResponseResult.Declined)
            {
                group.DismissInvite(invite);

                invite.Inviter.Send(new ServerGroupInviteResult
                {
                    GroupId = group.Id,
                    PlayerName = invite.Player.Name,
                    Result = InviteResult.Declined
                });

                // Remove Group and Members
                if (group.IsEmpty)
                    GlobalGroupManager.DisbandGroup(group);

                return;
            }

            var isNewGroup = group.IsEmpty;

            // Accepted. Add member and broadcast the info
            log.Info($"Add new member to the group");
            var newMember = group.AcceptInvite(invite);

            // Notify the inviter
            invite.Inviter.Send(new ServerGroupInviteResult
            {
                GroupId = request.GroupId,
                PlayerName = newMember.Player.Name,
                Result = InviteResult.Accepted
            });

            // if new group send ServerGroupJoin to all members
            if (isNewGroup)
            {
                group.Broadcast(member => group.BuildServerGroupJoin(member));
            }
            // otherwise send ServerGroupJoin to new member and ServerGroupMemberAdd to the rest
            else
            {
                newMember.Send(group.BuildServerGroupJoin(newMember));

                var addMember = group.BuildServerGroupMemberAdd(newMember);
                group.Broadcast(member => member.Id == newMember.Id ? null : addMember);
            }
        }

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            log.Info($"{session.Player.Name} is leaving group#{request.GroupId} invite scope {request.Scope}");

            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            // party leader disbanded
            if (request.Scope == GroupLeaveScope.Disband)
            {
                if (!member.IsPartyLeader)
                    return;

                log.Info($"Disbanding the group#{group.Id}");
                group.Broadcast(group.BuildServerGroupLeave(RemoveReason.Disband));
                GlobalGroupManager.DisbandGroup(group);
                return;
            }

            // remove leaver
            var wasPartyLeader = member.IsPartyLeader;
            group.RemoveMember(member);
            member.Send(group.BuildServerGroupLeave(RemoveReason.Left));

            // No more group members? Disband
            if (group.IsEmpty)
            {
                log.Info($"Disbanding the group#{group.Id} because it is empty");
                group.Broadcast(group.BuildServerGroupLeave(RemoveReason.Disband));
                GlobalGroupManager.DisbandGroup(group);
                return;
            }

            // PartyLeader left?
            if (wasPartyLeader)
            {
                var nextLeader = group.NextPartyLeaderCandidate;
                group.SetPartyLeader(nextLeader);

                // send promotion
                group.Broadcast(nextLeader.BuildServerGroupPromote());

                // send flags
                group.Broadcast(nextLeader.BuildServerGroupMemberFlagsChanged(true));
            }

            // broadcast about that member left
            group.Broadcast(member.BuildServerGroupRemove(RemoveReason.Left));
        }

        [MessageHandler(GameMessageOpcode.ClientGroupPromote)]
        public static void HandleGroupPromote(WorldSession session, ClientGroupPromote request)
        {
            log.Info($"{session.Player.Name} in group#{request.GroupId} promote another member");
            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            // not a party leader?
            if (!member.IsPartyLeader)
                return;

            // next group lead
            var newLeader = group.FindMember(request.PlayerIdentity);
            if (newLeader == null)
                return;

            // promote
            log.Info($"Promoting {newLeader.Player.Name} in party leader");
            var oldLeader = group.PartyLeader;
            group.SetPartyLeader(newLeader);

            // notify the members
            group.Broadcast(newLeader.BuildServerGroupPromote());
            group.Broadcast(oldLeader.BuildServerGroupMemberFlagsChanged(true));
            group.Broadcast(newLeader.BuildServerGroupMemberFlagsChanged(true));
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSetRole)]
        public static void HandleGroupSetRole(WorldSession session, ClientGroupSetRole request)
        {
            log.Info($"{session.Player.Name} in group#{request.GroupId} changing member flags: {request.ChangedFlag}");
            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            // player whose flags will change
            var targetPlayer = group.FindMember(request.PlayerIdentity);
            if (targetPlayer == null)
                return;

            // not allowed?
            if (!member.CanUpdateFlags(request.ChangedFlag, targetPlayer))
                return;

            // update the player flags
            var value = (request.Flags & request.ChangedFlag) == request.ChangedFlag;
            targetPlayer.SetFlags(request.ChangedFlag, value);

            // send responses
            log.Info($"{targetPlayer.Player.Name} in group#{group.Id} flags changed to {targetPlayer.Flags}");
            group.Broadcast(targetPlayer.BuildServerGroupMemberFlagsChanged(false));
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSendReadyCheck)]
        public static void HandleGroupSendReadyCheck(WorldSession session, ClientGroupSendReadyCheck request)
        {
            log.Info($"{session.Player.Name} in group#{request.GroupId} initiates ready check");
            var (member, group) = ValidateGroupMembership(session, request.GroupId);

            if (!member.CanReadyCheck)
                return;

            group.Members.ForEach(groupMember =>
            {
                groupMember.PrepareForReadyCheck();
                group.Broadcast(groupMember.BuildServerGroupMemberFlagsChanged(false));
            });

            var readyCheck = new ServerGroupSendReadyCheck
            {
                GroupId = group.Id,
                SenderIdentity = member.BuildTargetPlayerIdentity(),
                Message = request.Message
            };
            group.Broadcast(readyCheck);
        }

        /// <summary>
        /// Validate that current player is in a group with given group ID
        /// </summary>
        /// <returns>Tuple containing GroupMember and Group objects</returns>
        private static (GroupMember member, Group group) ValidateGroupMembership(WorldSession session, ulong groupId)
        {
            var member = session.Player.GroupMember;
            if (member == null)
                throw new InvalidPacketValueException();

            var group = member.Group;
            if (group == null || group.Id != groupId)
                throw new InvalidPacketValueException();

            return (member, group);
        }
    }
}
