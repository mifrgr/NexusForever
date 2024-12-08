using NexusForever.Game.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity.Player
{
    public class ClientRapidTransportHandler : IMessageHandler<IWorldSession, ClientRapidTransport>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public ClientRapidTransportHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientRapidTransport rapidTransport)
        {
            GameFormulaEntry gameFormulaEntry = gameTableManager.GameFormula.GetEntry(1307);
            if (gameFormulaEntry == null)
                return;

            uint publicTransportSpellId = gameFormulaEntry.Dataint0;
            if (session.Player.SpellManager.GetSpellCooldown(gameFormulaEntry.Dataint0) > 0d)
                publicTransportSpellId = gameFormulaEntry.Dataint01;

            session.Player.CastSpell(publicTransportSpellId, new SpellParameters
            {
                TaxiNode = rapidTransport.TaxiNode
            });
        }
    }
}
