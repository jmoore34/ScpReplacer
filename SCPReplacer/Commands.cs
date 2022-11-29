using System;
using System.Text.RegularExpressions;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;

namespace SCPReplacer
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Volunteer : ICommand
    {
        public string Command => "volunteer";

        public string[] Aliases => new[] { "v" };

        public string Description => "Volunteer to become a SCP that left at the beginning of the round";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 1)
            {
                response = Plugin.Singleton.Translation.WrongUsageMessage;
                return false;
            }

            if (Plugin.Singleton.HasReplacementCutoffPassed())
            {
                response = Plugin.Singleton.Translation.TooLateMessage;
                return false;
            }

            // Remove non-digits from input so they can type e.g. "SCP-079" and have it still work
            var requestedScp = arguments.FirstElement().ScpNumber();

            // Look in our list of SCPs awaiting replacement and see if any matches
            foreach (var role in Plugin.Singleton.ScpsAwaitingReplacement)
            {
                if (role.ScpNumber() == requestedScp)
                {
                    response = Plugin.Singleton.Translation.ChangedSuccessfullyMessage.Replace("%NUMBER%", requestedScp);
                    if (Player.Get(sender) is Player player)
                    {
                        player.Role = role;
                        Plugin.Singleton.ScpsAwaitingReplacement.Remove(role);

                        // Broadcast to everyone that the SCP has been replaced
                        // and give a slightly different message to the player that replaced the SCP
                        foreach (var p in Player.List)
                        {
                            if (p == player)
                            {
                                // For the player that replaced the SCP:
                                p.Broadcast(3, Plugin.Singleton.Translation.ChangedSuccessfullySelfBroadcast.Replace("%NUMBER%", requestedScp));
                                continue;
                            }
                            // for everyone else:
                            p.Broadcast(3, Plugin.Singleton.Translation.ChangedSuccessfullyEveryoneBroadcast.Replace("%NUMBER%", requestedScp));
                        }
                        Log.Info($"{player.Nickname} has replaced SCP-{requestedScp}");
                    }

                    // replacement successful
                    return true;
                }
            }

            // SCP was not in our list of SCPs awaiting replacement
            response = Plugin.Singleton.Translation.NotSuccessfully;
            return false;
        }
    }
}