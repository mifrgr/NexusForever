using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.HoverboardFlair)]
    public class PrerequisiteCheckHoverboardFlair : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckHoverboardFlair> log;

        public PrerequisiteCheckHoverboardFlair(
            ILogger<PrerequisiteCheckHoverboardFlair> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return player.PetCustomisationManager.GetCustomisation(PetType.HoverBoard, objectId) != null;
                case PrerequisiteComparison.NotEqual:
                    return player.PetCustomisationManager.GetCustomisation(PetType.HoverBoard, objectId) == null;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.HoverboardFlair}!");
                    return false;
            }
        }
    }
}
