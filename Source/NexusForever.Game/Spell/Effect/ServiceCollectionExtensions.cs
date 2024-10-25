using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Spell.Effect;

namespace NexusForever.Game.Spell.Effect
{
    public static class ServiceCollectionExtensions
    {
        public static void AddGameSpellEffect(this IServiceCollection sc)
        {
            sc.AddTransient<ISpellEffectHandlerFactory, SpellEffectHandlerFactory>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attribute = type.GetCustomAttribute<SpellEffectHandlerAttribute>();
                if (attribute == null)
                    continue;

                if (type.IsAssignableTo(typeof(ISpellEffectRemoveHandler)))
                    sc.AddKeyedTransient(typeof(ISpellEffectRemoveHandler), attribute.SpellEffectType, type);
                if (type.IsAssignableTo(typeof(ISpellEffectApplyHandler)))
                    sc.AddKeyedTransient(typeof(ISpellEffectApplyHandler), attribute.SpellEffectType, type);
            }
        }
    }
}
