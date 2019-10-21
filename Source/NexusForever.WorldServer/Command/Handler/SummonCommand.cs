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
            if (member is null)
            {
                await context.SendMessageAsync($"You are not in a group");
                return;
            }
            member.Group.Teleport(member.Player);
        }
    }
}
