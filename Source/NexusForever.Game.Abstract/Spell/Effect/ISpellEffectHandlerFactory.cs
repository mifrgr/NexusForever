using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Abstract.Spell.Effect
{
    public interface ISpellEffectHandlerFactory
    {
        /// <summary>
        /// Create a new <see cref="ISpellEffectApplyHandler"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        ISpellEffectApplyHandler CreateSpellEffectApplyHandler(SpellEffectType type);

        /// <summary>
        /// Create a new <see cref="ISpellEffectRemoveHandler"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        ISpellEffectRemoveHandler CreateSpellEffectRemovalHandler(SpellEffectType type);
    }
}
