using NexusForever.Game.Static.Entity;

namespace NexusForever.Network.World.Message
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class CastResultVitalAttribute : Attribute
    {
        public Vital Vital { get; }

        public CastResultVitalAttribute(Vital vital)
        {
            Vital = vital;
        }
    }
}
