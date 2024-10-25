using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Spell.Effect
{
    public class SpellEffectHandlerFactory : ISpellEffectHandlerFactory
    {
        #region Dependency Injection

        private readonly IServiceProvider serviceProvider;

        public SpellEffectHandlerFactory(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion

        /// <summary>
        /// Create a new <see cref="ISpellEffectApplyHandler"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public ISpellEffectApplyHandler CreateSpellEffectApplyHandler(SpellEffectType type)
        {
            return serviceProvider.GetKeyedService<ISpellEffectApplyHandler>(type);
        }

        /// <summary>
        /// Create a new <see cref="ISpellEffectRemoveHandler"/> for supplied <see cref="SpellEffectType"/>.
        /// </summary>
        public ISpellEffectRemoveHandler CreateSpellEffectRemovalHandler(SpellEffectType type)
        {
            return serviceProvider.GetKeyedService<ISpellEffectRemoveHandler>(type);
        }
    }
}
