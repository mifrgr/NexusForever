using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Proxy)]
    public class SpellEffectProxyHandler : ISpellEffectApplyHandler
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            // Some Proxies can be triggered only a certain amount of times per cast, by any target, and we evaluate all targets at once to apply Proxy effects.
            // This checks that value to ensure we've not exceeded the unique number of times this can fire.
            // A good example of this is for the Esper Ability Telekinetic Strike, it has a Proxy that grants Psi point when it hits an enemy.
            // However, Esper's can only generate a maximum of 1 Psi Point per cast. This tracks that value that seems to indicate it's a 1-time effect per cast.
            if (spell.GetEffectTriggerCount(info.Entry.Id, out uint count))
                if (count >= info.Entry.DataBits04)
                    return;

            spell.AddProxy(new Proxy(target, info.Entry, spell, spell.Parameters));
        }
    }
}
