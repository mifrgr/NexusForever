using System.Numerics;
using NexusForever.Game.Spell;
using NexusForever.Game.Static.Account;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network;
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
            TaxiNodeEntry taxiNode = gameTableManager.TaxiNode.GetEntry(rapidTransport.TaxiNode);
            if (taxiNode == null)
                throw new InvalidPacketValueException();

            WorldLocation2Entry worldLocation = gameTableManager.WorldLocation2.GetEntry(taxiNode.WorldLocation2Id);
            if (worldLocation == null)
                throw new InvalidPacketValueException();

            const ulong creditBasePriceEntry = 7;
            const ulong spellIdEntry = 1307;
            const ulong creditModifierEntry = 1310;

            TaxiRouteEntry taxiRoute = gameTableManager.TaxiRoute.GetEntry(creditBasePriceEntry);
            GameFormulaEntry spellEntry = gameTableManager.GameFormula.GetEntry(spellIdEntry);
            GameFormulaEntry costEntry = gameTableManager.GameFormula.GetEntry(creditModifierEntry);

            Vector3 destinationPosition = new Vector3(worldLocation.Position0, worldLocation.Position1, worldLocation.Position2);

            ulong creditBasePrice = taxiRoute.Price;
            ulong creditPrice;
            ulong serviceTokenPrice = taxiNode.ContentTier + 1;

            double cooldown = session.Player.SpellManager.GetSpellCooldown(spellEntry.Dataint0);


            //Implement right error, Spell4CastResult
            if (session.Player.Level < taxiNode.AutoUnlockLevel)
                throw new InvalidPacketValueException();


            if (session.Player.Map.Entry.Id == worldLocation.WorldId)
                creditPrice = (ulong)(creditBasePrice * costEntry.Dataint01 * taxiNode.ContentTier + Vector3.Distance(session.Player.Position, destinationPosition));
            else
                creditPrice = (ulong)(costEntry.Datafloat01 * taxiNode.ContentTier + creditBasePrice * costEntry.Dataint01 * taxiNode.ContentTier);


            //Check if Node is too Near
            if (Vector3.Distance(session.Player.Position, destinationPosition) < 200f)
                //ToDo : Implement right error, Spell4CastResult
                return;

            //Todo send to client Player has insufficient funds (Credits)
            if (!session.Player.CurrencyManager.CanAfford(CurrencyType.Credits, creditPrice) & cooldown == 0)
                //ToDo : Implement right error, Spell4CastResult
                return;

            //Todo send to client Player has insufficient funds (ServiceToken)
            if (!session.Player.Account.CurrencyManager.CanAfford(AccountCurrencyType.ServiceToken, serviceTokenPrice) & cooldown > 0)
                //ToDo : Implement right error, Spell4CastResult
                return;

            //Check requirement for RapidTransport(Credits)
            if (cooldown == 0 && session.Player.CurrencyManager.CanAfford(CurrencyType.Credits, creditPrice))
            {
                session.Player.CastSpell(spellEntry.Dataint0, new SpellParameters
                {
                    TaxiNode = rapidTransport.TaxiNode,
                    SpellCost = creditPrice

                });
                return;
            }


            //Check requirement for RapidTransport(ServiceToken)
            if (cooldown > 0 && session.Player.Account.CurrencyManager.CanAfford(AccountCurrencyType.ServiceToken, serviceTokenPrice))
            {
                session.Player.CastSpell(spellEntry.Dataint01, new SpellParameters
                {
                    TaxiNode = rapidTransport.TaxiNode,
                    SpellCost = serviceTokenPrice
                });
                return;
            }
        }
    }
}
