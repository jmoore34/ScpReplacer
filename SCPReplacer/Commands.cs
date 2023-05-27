using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Linq;

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
                    if (player.IsScp && player.Role != RoleTypeId.Scp0492)
                    {
                        response = "SCPs cannot use this command.";
                        return false;
                    }

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


    [CommandHandler(typeof(ClientCommandHandler))]
    public class HumanCommand : ICommand
    {
        public string Command => "human";

        public string[] Aliases => new string[] { "no" };
        public string Description => "Forfeit being an SCP and become a random human class instead (must be used near the start of the round)";


        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (Player.Get(sender) is Player scpPlayer)
            {
                if (!scpPlayer.IsScp)
                {
                    response = "You must be an SCP to use this command.";
                    return false;
                }
                if (scpPlayer.Role == RoleTypeId.Scp0492)
                {
                    response = "SCP 049-2 cannot use this command.";
                    return false;
                }
                var config = Plugin.Singleton.Config;
                var elapsedSeconds = Round.ElapsedTime.TotalSeconds;
                // Minimum required health (configurable percentage) of the SCP
                // when they quit to be eligible for replacement
                var requiredHealth = (int)(config.RequiredHealthPercentage / 100.0 * scpPlayer.MaxHealth);
                var customRole = scpPlayer.GetCustomRoles().FirstOrDefault();
                var scpNumber = customRole?.Name.ScpNumber() ?? scpPlayer.Role.ScpNumber();
                Log.Info($"{scpPlayer.Nickname} left {elapsedSeconds} seconds into the round, was SCP-{scpNumber} with {scpPlayer.Health}/{scpPlayer.MaxHealth} HP ({requiredHealth} required for replacement)");
                if (elapsedSeconds > config.QuitCutoff)
                {
                    response = "This command must be used closer to the start of the round.";
                    return false;
                }
                if (scpPlayer.Health < requiredHealth)
                {
                    response = "You are too low health to use this command.";
                    return false;
                }
                Plugin.Singleton.ScpLeft(scpPlayer);
                var newRole = UnityEngine.Random.value switch
                {
                    < 0.45f => RoleTypeId.ClassD,
                    < 0.9f => RoleTypeId.Scientist,
                    _ => RoleTypeId.FacilityGuard
                };
                response = $"You became a {newRole}";
                scpPlayer.Role.Set(newRole, Exiled.API.Enums.SpawnReason.LateJoin, RoleSpawnFlags.All);
                if (newRole is RoleTypeId.ClassD)
                {
                    scpPlayer.AddItem(ItemType.Flashlight);
                    scpPlayer.AddItem(ItemType.Coin);
                }
                foreach (CustomRole custom in scpPlayer.GetCustomRoles())
                    custom.RemoveRole(scpPlayer);
                scpPlayer.Broadcast(10, Plugin.Singleton.Translation.BroadcastHeader + $"You became a <color={newRole.GetColor().ToHex()}>{newRole.GetFullName()}</color>");
                return true;
            }
            else
            {
                response = "You must be a player to use this command";
                return false;
            }
        }
    }
}