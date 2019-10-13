using NexusForever.Shared.Network.Message;

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
