using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public interface ISpellEffectRemoveHandler
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info);
    }
}
