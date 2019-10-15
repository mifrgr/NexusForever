using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Static;
using NexusForever.WorldServer.Network.Message.Model;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using static NexusForever.WorldServer.Network.Message.Model.ServerGroupJoin;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    public partial class GroupMember
    {
        /// <summary>
        /// Send message to the given group member
        /// </summary>
        public void Send(IWritable message)
        {
            Player.Session.EnqueueMessageEncrypted(message);
        }

        /// <summary>
        /// Identity object suitable for sending to the client
        /// </summary>
        public TargetPlayerIdentity BuildTargetPlayerIdentity()
        {
            return new TargetPlayerIdentity
            {
                RealmId = WorldServer.RealmId,
                CharacterId = Player.CharacterId
            };
        }

        /// <summary>
        /// Group member info suitable for sending to the client
        /// </summary>
        public GroupMemberInfo BuildGroupMemberInfo(uint groupIndex)
        {
            return new GroupMemberInfo
            {
                MemberIdentity = BuildTargetPlayerIdentity(),
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
            return new Member
            {
                Name = Player.Name,
                Faction = Player.Faction1,
                Race = Player.Race,
                Class = Player.Class,
                Sex = Player.Sex,
                Level = (byte)Player.Level,
                EffectiveLevel = (byte)Player.Level,
                Path = Player.Path,
                GroupMemberId = Id,
                Realm = WorldServer.RealmId,
                WorldZoneId = (ushort)Player.Zone.Id,
                Unknown25 = 0, // Player.Map.Entry.Id,
                Unknown26 = 0,
                SyncedToGroup = true
            };
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
                PlayerIdentity = BuildTargetPlayerIdentity()
            };
        }

        /// <summary>
        /// Build flags flag change
        /// </summary>
        /// <param name="fromPromotion">if true client will supress notifications in chat</param>
        /// <returns></returns>
        public ServerGroupMemberFlagsChanged BuildServerGroupMemberFlagsChanged(bool fromPromotion)
        {
            return new ServerGroupMemberFlagsChanged
            {
                GroupId = Group.Id,
                MemberId = Id,
                PlayerIdentity = BuildTargetPlayerIdentity(),
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
                PlayerLeave = BuildTargetPlayerIdentity(),
                RemoveReason = reason
            };
        }
    }
}
