using System.Numerics;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Static.Account;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.RapidTransport)]
    public class SpellEffectRapidTransportHandler : ISpellEffectApplyHandler
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;
        private readonly IRapidTransportCostCalculator rapidTransportCostCalculator;

        public SpellEffectRapidTransportHandler(
            IGameTableManager gameTableManager,
            IRapidTransportCostCalculator rapidTransportCostCalculator)
        {
            this.gameTableManager             = gameTableManager;
            this.rapidTransportCostCalculator = rapidTransportCostCalculator;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            if (spell.Parameters.TaxiNode == null)
                return;

            TaxiNodeEntry taxiNodeEntry = gameTableManager.TaxiNode.GetEntry(spell.Parameters.TaxiNode.Value);
            if (taxiNodeEntry == null)
                return;

            WorldLocation2Entry worldLocationEntry = gameTableManager.WorldLocation2.GetEntry(taxiNodeEntry.WorldLocation2Id);
            if (worldLocationEntry == null)
                return;

            GameFormulaEntry spellEntry = gameTableManager.GameFormula.GetEntry(1307);
            if (spellEntry == null)
                return;

            if (target is not IPlayer player)
                return;

            if (!player.CanTeleport())
                return;

            if (player.SpellManager.GetSpellCooldown(spellEntry.Dataint0) > 0d)
            {
                ulong? serviceTokenPrice = rapidTransportCostCalculator.CalculateServiceTokenCost(spell.Parameters.TaxiNode.Value);
                if (serviceTokenPrice == null)
                    return;

                if (!player.Account.CurrencyManager.CanAfford(AccountCurrencyType.ServiceToken, serviceTokenPrice.Value))
                    return;

                player.Account.CurrencyManager.CurrencySubtractAmount(AccountCurrencyType.ServiceToken, serviceTokenPrice.Value);
            }
            else
            {
                ulong? creditPrice = rapidTransportCostCalculator.CalculateCreditCost(spell.Parameters.TaxiNode.Value, player.Map.Entry.Id, player.Position);
                if (creditPrice == null)
                    return;

                if (!player.CurrencyManager.CanAfford(CurrencyType.Credits, creditPrice.Value))
                    return;

                player.CurrencyManager.CurrencySubtractAmount(CurrencyType.Credits, creditPrice.Value);
            }

            var rotation = new Quaternion(worldLocationEntry.Facing0, worldLocationEntry.Facing0, worldLocationEntry.Facing2, worldLocationEntry.Facing3);
            player.Rotation = rotation.ToEuler();
            player.TeleportTo((ushort)worldLocationEntry.WorldId, worldLocationEntry.Position0, worldLocationEntry.Position1, worldLocationEntry.Position2);
        }
    }
}
