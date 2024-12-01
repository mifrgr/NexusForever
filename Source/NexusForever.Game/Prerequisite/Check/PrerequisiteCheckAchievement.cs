using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Achievement)]
    public class PrerequisiteCheckAchievement : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckAchievement> log;

        public PrerequisiteCheckAchievement(
            ILogger<PrerequisiteCheckAchievement> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.NotEqual:
                    return !player.AchievementManager.HasCompletedAchievement((ushort)objectId);
                case PrerequisiteComparison.Equal:
                    return player.AchievementManager.HasCompletedAchievement((ushort)objectId);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Achievement}!");
                    return false;
            }
        }
    }
}
