using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public interface ISpellEffectApplyHandler
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info);
    }
}
