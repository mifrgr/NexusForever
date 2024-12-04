using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Unknown194)]
    public class PrerequisiteCheckUnknown194 : IPrerequisiteCheck
    {
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId)
        {
            // TODO: Only used in Mount check prerequisites. Its use is unknown.

            return true;
        }
    }
}
