using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPReplacer
{
    [CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class Volunteer : ICommand
    {
        public string Command => "volunteer";

        public string[] Aliases => new[] { "v" };

        public string Description => "Volunteer to become a SCP that left at the beginning of the round";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Map.Broadcast(
                new Exiled.API.Features.Broadcast("<color=yellow>[Chaos Theory SCP Replacer]</color>\nEnter <color=green>.volunteer 079</color> in the <color=blue>~</color> console to become <color=red>SCP-079</color>"));
            response = "Recieved .volunteer";
            return true;
        }
    }
}
