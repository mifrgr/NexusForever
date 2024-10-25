using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpellEffectHandlerAttribute : Attribute
    {
        public SpellEffectType SpellEffectType { get; }

        public SpellEffectHandlerAttribute(SpellEffectType spellEffectType)
        {
            SpellEffectType = spellEffectType;
        }
    }
}
