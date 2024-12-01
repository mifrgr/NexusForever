using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Prerequisite)]
    public class PrerequisiteCheckPrerequisite : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckPrerequisite> log;
        private readonly IPrerequisiteManager prerequisiteManager;

        public PrerequisiteCheckPrerequisite(
            ILogger<PrerequisiteCheckPrerequisite> log,
            IPrerequisiteManager prerequisiteManager)
        {
            this.log                 = log;
            this.prerequisiteManager = prerequisiteManager;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.NotEqual:
                    return !prerequisiteManager.Meets(player, objectId);
                case PrerequisiteComparison.Equal:
                    return prerequisiteManager.Meets(player, objectId);
                default:
                    log.LogWarning($"Unhandled {comparison} for {PrerequisiteType.Prerequisite}!");
                    return false;
            }
        }
    }
}
