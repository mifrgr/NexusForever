using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Extensions;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using static NexusForever.WorldServer.Network.Message.Model.ServerGroupJoin;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    public sealed partial class GroupMember
    {
        /// <summary>
        /// Send message to the given group member
        /// </summary>
        public void Send(IWritable message)
        {
            Player.Session.EnqueueMessageEncrypted(message);
        }

        /// <summary>
        /// Group member info suitable for sending to the client
        /// </summary>
        public GroupMemberInfo BuildGroupMemberInfo(uint groupIndex)
        {
            return new GroupMemberInfo
            {
                MemberIdentity = Player.BuildTargetPlayerIdentity(),
                Flags = Flags,
                GroupMember = BuildGroupMember(),
                GroupIndex = groupIndex
            };
        }

        /// <summary>
        /// Build Member object that is sendable to the client
        /// </summary>
        public Member BuildGroupMember()
        {
            return Player.BuildGroupMember(Id);
        }

        /// <summary>
        /// Build Group promote packet
        /// </summary>
        public ServerGroupPromote BuildServerGroupPromote()
        {
            return new ServerGroupPromote
            {
                GroupId = Group.Id,
                MemberId = Id,
                PlayerIdentity = Player.BuildTargetPlayerIdentity()
            };
        }

        /// <summary>
        /// Build flags flag change
        /// </summary>
        /// <param name="fromPromotion">if true client will supress notifications in chat</param>
        public ServerGroupMemberFlagsChanged BuildServerGroupMemberFlagsChanged(bool fromPromotion)
        {
            return new ServerGroupMemberFlagsChanged
            {
                GroupId = Group.Id,
                MemberId = Id,
                PlayerIdentity = Player.BuildTargetPlayerIdentity(),
                Flags = Flags,
                FromPromotion = fromPromotion
            };
        }

        /// <summary>
        /// Build Group Remove packet with the given reason
        /// </summary>
        public ServerGroupRemove BuildServerGroupRemove(RemoveReason reason)
        {
            return new ServerGroupRemove
            {
                GroupId = Group.Id,
                MemberId = Id,
                PlayerLeave = Player.BuildTargetPlayerIdentity(),
                RemoveReason = reason
            };
        }


        /// <summary>
        /// Build player entity update packet to associate it with group id
        /// </summary>
        /// <param name="isMember">Sends group id, otherwise sense 0</param>
        public ServerEntityGroupAssociation BuildServerEntityGroupAssociation(bool isMember)
        {
            return new ServerEntityGroupAssociation
            {
                UnitId = Player.Guid,
                GroupId = isMember ? Group.Id : 0ul
            };
        }

        /// <summary>
        /// Build group join referral
        /// </summary>
        /// <param name="inviteeName">Player name being invited</param>
        public ServerGroupReferral BuildServerGroupReferral(string inviteeName)
        {
            return new ServerGroupReferral
            {
                GroupId = Group.Id,
                inviter = Player.BuildTargetPlayerIdentity(),
                inviteeName = inviteeName
            };
        }
    }
}
