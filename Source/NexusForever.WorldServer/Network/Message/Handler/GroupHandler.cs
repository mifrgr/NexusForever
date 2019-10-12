using NexusForever.Shared.Network;
using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Group;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class GroupHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [MessageHandler(GameMessageOpcode.ClientGroupInvite)]
        public static void HandleGroupInvite(WorldSession session, ClientGroupInvite request)
        {
            var player = session.Player;
            var group = player.GroupMember?.Group ?? GlobalGroupManager.CreateGroup(player);
            group.Invite(player, request.PlayerName);            
        }
                
        [MessageHandler(GameMessageOpcode.ClientGroupInviteResponse)]
        public static void HandleGroupInviteResponse(WorldSession session, ClientGroupInviteResponse request)
        {
            var player = session.Player;
            
            var invite = player.GroupInvite;
            if (invite == null)
                return;

            invite.Group.HandleInvite(invite, request.Response);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupLeave)]
        public static void HandleGroupLeave(WorldSession session, ClientGroupLeave request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.Leave(player, request.Scope);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupPromote)]
        public static void HandleGroupPromote(WorldSession session, ClientGroupPromote request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.Promote(player, request.PlayerIdentity);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSetRole)]
        public static void HandleGroupSetRole(WorldSession session, ClientGroupSetRole request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.UpdateFlags(player, request.PlayerIdentity, request.ChangedFlags, request.Flags);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupSendReadyCheck)]
        public static void HandleGroupSendReadyCheck(WorldSession session, ClientGroupSendReadyCheck request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.ReadyCheck(player, request.Message);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupFlagsChanged)]
        public static void HandleGroupFlagsChanged(WorldSession session, ClientGroupFlags request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.SetFlags(request.Flag);
        }

        [MessageHandler(GameMessageOpcode.ClientGroupKick)]
        public static void HandleGroupKick(WorldSession session, ClientGroupKick request)
        {
            var (group, player) = ValidateGroupMembership(session, request.GroupId);
            group.Kick(player, request.PlayerIdentity);
        }

        // <summary>
        // Validate that current player is in a group with given group ID
        // </summary>
        // <returns>Tuple containing GroupMember and Group objects</returns>
        private static (Group, Player) ValidateGroupMembership(WorldSession session, ulong groupId)
        {
            var player = session.Player;
            var member = player.GroupMember;
            if (member == null)
                throw new InvalidPacketValueException();

            var group = member.Group;
            if (group == null || group.Id != groupId)
                throw new InvalidPacketValueException();

            return (group, player);
        }
    }
}
