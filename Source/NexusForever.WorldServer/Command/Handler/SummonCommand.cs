using System.Threading.Tasks;
using NexusForever.WorldServer.Command.Attributes;
using NexusForever.WorldServer.Command.Contexts;

namespace NexusForever.WorldServer.Command.Handler
{
    [Name("Summon")]
    public class SummonCommand : NamedCommand
    {

        public override string HelpText => "Summons group members to your location";

        public SummonCommand()
            : base(true, "summon")
        {
        }

        protected override async Task HandleCommandAsync(CommandContext context, string command, string[] parameters)
        {
            var member = context.Session.Player.GroupMember;
            if (member == null)
            {
                await context.SendMessageAsync($"You are not in a group");
                return;
            }

            var worldId = (ushort)context.Session.Player.Map.Entry.Id;
            var x = context.Session.Player.Position.X;
            var y = context.Session.Player.Position.Y;
            var z = context.Session.Player.Position.Z;

            foreach (var groupMember in member.Group.Members)
            {
                if (groupMember.Id == member.Id)
                    continue;
                groupMember.Player.TeleportTo(worldId, x, y, z);
            }
        }
    }
}
