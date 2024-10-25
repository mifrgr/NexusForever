using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.UnitPropertyModifier)]
    public class SpellEffectUnitPropertyModifierHandler : ISpellEffectApplyHandler, ISpellEffectRemoveHandler
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            // TODO: I suppose these could be cached somewhere instead of generating them every single effect?
            SpellPropertyModifier modifier =
                new SpellPropertyModifier((Property)info.Entry.DataBits00,
                    info.Entry.DataBits01,
                    BitConverter.UInt32BitsToSingle(info.Entry.DataBits02),
                    BitConverter.UInt32BitsToSingle(info.Entry.DataBits03),
                    BitConverter.UInt32BitsToSingle(info.Entry.DataBits04));
            target.AddSpellModifierProperty(modifier, spell.Parameters.SpellInfo.Entry.Id);
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            target.RemoveSpellProperty((Property)info.Entry.DataBits00, spell.Parameters.SpellInfo.Entry.Id);
        }
    }
}
