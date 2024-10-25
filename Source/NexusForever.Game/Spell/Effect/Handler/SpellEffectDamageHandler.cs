using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Damage)]
    public class SpellEffectDamageHandler : ISpellEffectApplyHandler
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            if (!target.CanAttack(spell.Caster))
                return;

            // TODO: Merge DamageCalculator, uncomment below lines, and delete the hardcoded values before target takes damage.

            // uint damage = 0;
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType00, info.Entry.ParameterValue00);
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType01, info.Entry.ParameterValue01);
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType02, info.Entry.ParameterValue02);
            // damage += DamageCalculator.Instance.GetBaseDamageForSpell(caster, info.Entry.ParameterType03, info.Entry.ParameterValue03);

            // DamageCalculator.Instance.CalculateDamage(caster, target, this, info, (DamageType)info.Entry.DamageType, damage);

            info.AddDamage((DamageType)info.Entry.DamageType, 50);
            info.Damage.ShieldAbsorbAmount = 25;
            info.Damage.AdjustedDamage = 50;

            // TODO: Deal damage
            target.TakeDamage(spell.Caster, info.Damage);
        }
    }
}
