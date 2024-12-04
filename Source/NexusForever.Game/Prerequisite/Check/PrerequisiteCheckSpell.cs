using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    public class PrerequisiteCheckSpell : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckSpell> log;

        public PrerequisiteCheckSpell(
            ILogger<PrerequisiteCheckSpell> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            if (value == 0 && objectId == 0)
                return false;

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return player.HasSpell(s => s.Spell4Id == value, out ISpell equalSpell);
                case PrerequisiteComparison.NotEqual:
                    return !player.HasSpell(s => s.Spell4Id == value, out ISpell notEqualSpell);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Spell}!");
                    return false;
            }
        }
    }
}
