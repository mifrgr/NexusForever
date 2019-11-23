using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.VitalModifier)]
    public class SpellEffectVitalModifierHandler : ISpellEffectApplyHandler
    {
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            Vital vital = (Vital)info.Entry.DataBits00;
            target.ModifyVital(vital, info.Entry.DataBits01);
        }
    }
}
