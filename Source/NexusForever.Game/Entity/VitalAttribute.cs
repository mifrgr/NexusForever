using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Entity
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class VitalAttribute : Attribute
    {
        public Vital Vital { get; }

        public VitalAttribute(Vital vital)
        {
            Vital = vital;
        }
    }
}
