using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Abstract.Spell
{
    public interface ISpellLookupManager
    {
        bool TryGetDashSpell(DashDirection direction, out uint dashSpellId);
    }
}