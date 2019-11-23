using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Spell.Effect;
using NexusForever.Shared;

namespace NexusForever.Game.Spell
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameSpell(this IServiceCollection sc)
        {
            sc.AddGameSpellEffect();

            sc.AddSingletonLegacy<IGlobalSpellManager, GlobalSpellManager>();
            sc.AddSingletonLegacy<ISpellLookupManager, SpellLookupManager>();
        }
    }
}
