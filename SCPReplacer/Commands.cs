using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            // This should not happen, but this check is here for defensiveness
            if (Plugin.Instance == null)
            {
                Log.Error("Plugin.Instance was null in .volunteer command call. This should not happen; contact SCP Replace developer.");
                response = "SCP Replace not currently enabled or working.";
                return false;
            }
            if (arguments.Count != 1)
            {
                response = "Usage: .volunteer <SCP number>. Example: .volunteer 079 or .v 079";
                return false;
            }
            if (Plugin.Instance.HasReplacementCutoffPassed())
            {
                response = "It is too late in the game to replace an SCP.";
                return false;
            }
            // Remove non-digits from input so they can type e.g. "SCP-079" and have it still work
            string requestedSCP = Regex.Replace(arguments.FirstElement(), @"[^0-9]", "");
            
            // Look in our list of SCPs awaiting replacement and see if any matches
            foreach (Exiled.API.Features.Roles.Role role in Plugin.Instance.ScpsAwaitingReplacement)
            {
                if (role.ScpNumber() == requestedSCP)
                {
                    response = $"Changing your class to SCP-{requestedSCP}";
                    if (sender is PlayerCommandSender playerSender)
                    {
                        Player player = Player.Get(playerSender.PlayerId);
                        player.SetRole(role, Exiled.API.Enums.SpawnReason.ForceClass);
                        Plugin.Instance.ScpsAwaitingReplacement.Remove(role);

                        // Broadcast to everyone that the SCP has been replaced
                        // and give a slightly different message to the player that replaced the SCP
                        foreach (Player p in Player.List)
                        {
                            if (p.Id == playerSender.PlayerId)
                            {
                                p.Broadcast(new Exiled.API.Features.Broadcast(
                                    $"{Plugin.Instance.BroadcastHeader()}You have replaced <color=red>SCP-{requestedSCP}</color>", 3, true));
                            }
                            else
                            {
                                // for everyone else:
                                p.Broadcast(new Exiled.API.Features.Broadcast(
                                    $"{Plugin.Instance.BroadcastHeader()}<color=red>SCP-{requestedSCP}</color> has been replaced", 3, true));
                            }
                        }

                        Log.Info($"{player.Nickname} has replaced SCP-{requestedSCP}");
                    }
                    // replacement successful
                    return true;
                }
            }
            // SCP was not in our list of SCPs awaiting replacement
            response = "Unable to find a recently quit SCP with that SCP number";
            return false;
        }
    }
}
