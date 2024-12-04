using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Shared;

namespace NexusForever.Game.Prerequisite
{
    public class PrerequisiteManager : Singleton<PrerequisiteManager>, IPrerequisiteManager
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteManager> log;
        private readonly IServiceProvider serviceProvider;
        private readonly IGameTableManager gameTableManager;

        public PrerequisiteManager(
            ILogger<PrerequisiteManager> log,
            IServiceProvider serviceProvider,
            IGameTableManager gameTableManager)
        {
            this.log              = log;
            this.serviceProvider  = serviceProvider;
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Checks if <see cref="IPlayer"/> meets supplied prerequisite.
        /// </summary>
        public bool Meets(IPlayer player, uint prerequisiteId)
        {
            PrerequisiteEntry entry = gameTableManager.Prerequisite.GetEntry(prerequisiteId);
            if (entry == null)
                throw new ArgumentException();
            
            switch (entry.Flags)
            {
                case EvaluationMode.EvaluateAND:
                    return MeetsEvaluateAnd(player, prerequisiteId, entry);
                case EvaluationMode.EvaluateOR:
                    return MeetsEvaluateOr(player, prerequisiteId, entry);
                default:
                    log.LogTrace($"Unhandled EvaluationMode {entry.Flags}");
                    return false;
            }
        }

        private bool MeetsEvaluateAnd(IPlayer player, uint prerequisiteId, PrerequisiteEntry entry)
        {
            for (int i = 0; i < entry.PrerequisiteTypeId.Length; i++)
            {
                PrerequisiteType type = entry.PrerequisiteTypeId[i];
                if (type == PrerequisiteType.None)
                    continue;

                PrerequisiteComparison comparison = (PrerequisiteComparison)entry.PrerequisiteComparisonId[i];
                if (!Meets(player, type, comparison, entry.Value[i], entry.ObjectId[i]))
                {
                    log.LogTrace($"Player {player.Name} failed prerequisite AND check ({prerequisiteId}) {type}, {comparison}, {entry.Value[i]}, {entry.ObjectId[i]}");
                    return false;
                }
            }

            return true;
        }

        private bool MeetsEvaluateOr(IPlayer player, uint prerequisiteId, PrerequisiteEntry entry)
        {
            for (int i = 0; i < entry.PrerequisiteTypeId.Length; i++)
            {
                PrerequisiteType type = entry.PrerequisiteTypeId[i];
                if (type == PrerequisiteType.None)
                    continue;

                if (Meets(player, type, entry.PrerequisiteComparisonId[i], entry.Value[i], entry.ObjectId[i]))
                    return true;
            }

            log.LogTrace($"Player {player.Name} failed prerequisite OR check ({prerequisiteId})");
            return false;
        }

        private bool Meets(IPlayer player, PrerequisiteType type, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            IPrerequisiteCheck handler = serviceProvider.GetKeyedService<IPrerequisiteCheck>(type);
            if (handler == null)
            {
                log.LogWarning($"Unhandled PrerequisiteType {type}!");
                return false;
            }

            return handler.Meets(player, comparison, value, objectId);
        }
    }
}
