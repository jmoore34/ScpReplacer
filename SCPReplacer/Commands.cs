using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using MEC;
using System;

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
                if (role.Name == requestedScp && Player.Get(sender) is Player player)
                {
                    role.Volunteers.Add(player);

                    if (role.LotteryTimeout == null)
                    {
                        role.LotteryTimeout = Timing.CallDelayed(Plugin.Singleton.Config.LotteryPeriodSeconds, () =>
                        {
                            role.Replace();
                        });
                    }

                    response = $"You have entered the lottery to become SCP {role.Name}.";
                    player.Broadcast(5, Plugin.Singleton.Translation.BroadcastHeader +
                                        Plugin.Singleton.Translation.EnteredLotteryBroadcast.Replace("%NUMBER%", requestedScp),
                                        Broadcast.BroadcastFlags.Normal,
                                        true // Clear previous broadcast to overwrite lingering volunteer opportunity message
                                        );
                    // replacement successful
                    return true;
                }
            }

            // SCP was not in our list of SCPs awaiting replacement
            if (Plugin.Singleton.ScpsAwaitingReplacement.IsEmpty())
            {
                response = Plugin.Singleton.Translation.NoEligibleSCPsError;
            }
            else
            {
                response = Plugin.Singleton.Translation.InvalidSCPError
                     + string.Join(", ", Plugin.Singleton.ScpsAwaitingReplacement); // display available SCP numbers
            }
            return false;
        }
    }
}
