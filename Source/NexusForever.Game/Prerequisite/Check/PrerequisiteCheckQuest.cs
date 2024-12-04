using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Quest)]
    public class PrerequisiteCheckQuest : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckQuest> log;

        public PrerequisiteCheckQuest(
            ILogger<PrerequisiteCheckQuest> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return player.QuestManager.GetQuestState((ushort)objectId) == (QuestState)value;
                case PrerequisiteComparison.NotEqual:
                    return player.QuestManager.GetQuestState((ushort)objectId) != (QuestState)value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Quest}!");
                    return false;
            }
        }
    }
}
