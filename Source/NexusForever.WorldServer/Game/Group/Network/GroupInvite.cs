using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model.Shared;
using static NexusForever.WorldServer.Network.Message.Model.ServerGroupJoin;

#nullable enable

namespace NexusForever.WorldServer.Game.Group
{
    public partial class GroupInvite
    {
        /// <summary>
        /// Send message to the invited player
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
        public GroupMemberInfo BuildGroupMemberInfo()
        {
            return new GroupMemberInfo
            {
                MemberIdentity = BuildTargetPlayerIdentity(),
                Flags = 0,
                GroupMember = BuildGroupMember(),
                GroupIndex = 0
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
                GroupMemberId = 0,
                Realm = WorldServer.RealmId,
                WorldZoneId = (ushort)Player.Zone.Id,
                Unknown25 = 0, // Player.Map.Entry.Id,
                Unknown26 = 0,
                SyncedToGroup = true
            };
        }
    }
}
