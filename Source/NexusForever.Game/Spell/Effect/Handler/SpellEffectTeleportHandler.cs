using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Housing;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Teleport)]
    public class SpellEffectTeleportHandler : ISpellEffectApplyHandler
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;
        private readonly IGlobalResidenceManager globalResidenceManager;
        private readonly IMapLockManager mapLockManager;

        public SpellEffectTeleportHandler(
            IGameTableManager gameTableManager,
            IGlobalResidenceManager globalResidenceManager,
            IMapLockManager mapLockManager)
        {
            this.gameTableManager       = gameTableManager;
            this.globalResidenceManager = globalResidenceManager;
            this.mapLockManager         = mapLockManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Apply(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info)
        {
            if (target is not IPlayer player)
                return;

            WorldLocation2Entry locationEntry = gameTableManager.WorldLocation2.GetEntry(info.Entry.DataBits00);
            if (locationEntry == null)
                return;

            // Handle Housing Teleport
            if (locationEntry.WorldId == 1229)
            {
                IResidence residence = globalResidenceManager.GetResidenceByOwner(player.Name);
                if (residence == null)
                    residence = globalResidenceManager.CreateResidence(player);

                IResidenceEntrance entrance = globalResidenceManager.GetResidenceEntrance(residence.PropertyInfoId);
                if (player.CanTeleport())
                {
                    IMapLock mapLock = mapLockManager.GetResidenceLock(residence.Parent ?? residence);

                    player.Rotation = entrance.Rotation.ToEulerDegrees();
                    player.TeleportTo(entrance.Entry, entrance.Position, mapLock);
                    return;
                }
            }

            if (player.CanTeleport())
            {
                player.Rotation = new Quaternion(locationEntry.Facing0, locationEntry.Facing1, locationEntry.Facing2, locationEntry.Facing3).ToEulerDegrees();
                player.TeleportTo((ushort)locationEntry.WorldId, locationEntry.Position0, locationEntry.Position1, locationEntry.Position2);
            }
        }
    }
}
