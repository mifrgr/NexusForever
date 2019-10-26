using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Game.Group.Extensions;
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
        /// Group member info suitable for sending to the client
        /// </summary>
        public GroupMemberInfo BuildGroupMemberInfo()
        {
            return new GroupMemberInfo
            {
                MemberIdentity = Player.BuildTargetPlayerIdentity(),
                Flags = 0,
                GroupMember = Player.BuildGroupMember(0),
                GroupIndex = 0
            };
        }
    }
}
