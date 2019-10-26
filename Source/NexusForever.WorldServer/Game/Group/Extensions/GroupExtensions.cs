using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Network.Message.Model.Shared;

namespace NexusForever.WorldServer.Game.Group.Extensions
{
    public static partial class GroupExtensions
    {
        /// <summary>
        /// Build Member object that is sendable to the client
        /// </summary>
        public static Member BuildGroupMember(this Player player, ushort groupMemberId)
        {
            return new Member
            {
                Name = player.Name,
                Faction = player.Faction1,
                Race = player.Race,
                Class = player.Class,
                Sex = player.Sex,
                Level = (byte)player.Level,
                EffectiveLevel = (byte)player.Level,
                Path = player.Path,
                GroupMemberId = groupMemberId,
                Realm = WorldServer.RealmId,
                WorldZoneId = (ushort)player.Zone.Id,
                Unknown25 = player.Map.Entry.Id, // probably?
                Unknown26 = 1, // instance / phase?
                SyncedToGroup = true
            };
        }

        /// <summary>
        /// Identity object suitable for sending to the client
        /// </summary>
        public static TargetPlayerIdentity BuildTargetPlayerIdentity(this Player player)
        {
            return new TargetPlayerIdentity
            {
                RealmId = WorldServer.RealmId,
                CharacterId = player.CharacterId
            };
        }
    }
}
