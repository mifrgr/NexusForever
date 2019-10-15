using NexusForever.Shared.Network.Message;

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
    }
}
