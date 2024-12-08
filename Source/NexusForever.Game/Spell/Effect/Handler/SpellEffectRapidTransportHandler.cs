﻿using System.Numerics;
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

        public SpellEffectRapidTransportHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            TaxiNodeEntry taxiNode = gameTableManager.TaxiNode.GetEntry(spell.Parameters.TaxiNode);
            if (taxiNode == null)
                return;

            WorldLocation2Entry worldLocation = gameTableManager.WorldLocation2.GetEntry(taxiNode.WorldLocation2Id);
            if (worldLocation == null)
                return;

            const uint spellIdRapidTransportCredits = 82922;
            const uint spellIdRapidTransportServiceToken = 82956;

            if (target is not IPlayer player)
                return;

            if (!player.CanTeleport())
                return;

            if (info.Entry.SpellId == spellIdRapidTransportCredits && player.CurrencyManager.CanAfford(CurrencyType.Credits, spell.Parameters.SpellCost))
                player.CurrencyManager.CurrencySubtractAmount(CurrencyType.Credits, spell.Parameters.SpellCost);
            else if (info.Entry.SpellId == spellIdRapidTransportServiceToken && player.Account.CurrencyManager.CanAfford(AccountCurrencyType.ServiceToken, spell.Parameters.SpellCost))
                player.Account.CurrencyManager.CurrencySubtractAmount(AccountCurrencyType.ServiceToken, spell.Parameters.SpellCost);
            else //ToDo : Implement right error, Spell4CastResult
                return;

            var rotation = new Quaternion(worldLocation.Facing0, worldLocation.Facing0, worldLocation.Facing2, worldLocation.Facing3);
            player.Rotation = rotation.ToEuler();
            player.TeleportTo((ushort)worldLocation.WorldId, worldLocation.Position0, worldLocation.Position1, worldLocation.Position2);
        }
    }
}
