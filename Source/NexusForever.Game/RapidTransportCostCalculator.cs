using System.Numerics;
using NexusForever.Game.Abstract;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game
{
    public class RapidTransportCostCalculator : IRapidTransportCostCalculator
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public RapidTransportCostCalculator(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public ulong? CalculateCreditCost(uint taxiNodeId, uint worldId, Vector3 position)
        {
            TaxiNodeEntry taxiNodeEntry = gameTableManager.TaxiNode.GetEntry(taxiNodeId);
            if (taxiNodeEntry == null)
                return null;

            WorldLocation2Entry worldLocationEntry = gameTableManager.WorldLocation2.GetEntry(taxiNodeEntry.WorldLocation2Id);
            if (worldLocationEntry == null)
                return null;

            var destinationPosition = new Vector3(worldLocationEntry.Position0, worldLocationEntry.Position1, worldLocationEntry.Position2);

            GameFormulaEntry costEntry = gameTableManager.GameFormula.GetEntry(1310);
            if (costEntry == null)
                return null;

            TaxiRouteEntry taxiRouteEntry = gameTableManager.TaxiRoute.GetEntry(7);
            if (taxiRouteEntry == null)
                return null;

            ulong creditPrice;
            if (worldId == worldLocationEntry.WorldId)
                creditPrice = (ulong)(taxiRouteEntry.Price * costEntry.Dataint01 * taxiNodeEntry.ContentTier + Vector3.Distance(position, destinationPosition));
            else
                creditPrice = (ulong)(costEntry.Datafloat01 * taxiNodeEntry.ContentTier + taxiRouteEntry.Price * costEntry.Dataint01 * taxiNodeEntry.ContentTier);

            return creditPrice;
        }

        public ulong? CalculateServiceTokenCost(uint taxiNodeId)
        {
            TaxiNodeEntry taxiNodeEntry = gameTableManager.TaxiNode.GetEntry(taxiNodeId);
            if (taxiNodeEntry == null)
                return null;

            return taxiNodeEntry.ContentTier + 1;
        }
    }
}
