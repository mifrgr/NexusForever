using System.Numerics;

namespace NexusForever.Game.Abstract
{
    public interface IRapidTransportCostCalculator
    {
        ulong? CalculateCreditCost(uint taxiNodeId, uint worldId, Vector3 position);
        ulong? CalculateServiceTokenCost(uint taxiNodeId);
    }
}
