using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCPReplacer
{
    public class Plugin : Plugin<Config, Translations>
    {
        public override string Name => "SCP Replacer";
        public override string Author => "Jon M";
        public override Version Version => new Version(1, 0, 0);

        // Singleton pattern allows easy access to the central state from other classes
        // (e.g. commands)
        public static Plugin Singleton { get; private set; }

        // A list of Roles (SCPs) that have quit early in the round and have not yet been replaced
        public List<Exiled.API.Features.Roles.Role> ScpsAwaitingReplacement { get; } = new List<Exiled.API.Features.Roles.Role>();

        // A stoppable handle on the coroutine that performs AFK checks every second
        private CoroutineHandle coroutine { get; set; }

        // Detect AFK by keeping track of how long each player has kept their rotation
        private Dictionary<Player, PlayerData> playerDataMap = new Dictionary<Player, PlayerData>();

        public override void OnEnabled()
        {
            // Set up the Singleton so we can easily get the instance with all the state
            // from another class.
            Singleton = this;

            // Register event handlers
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded += OnRoundEnded;
            Exiled.Events.Handlers.Player.Destroying += OnDestroying;
            Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            // Deregister event handlers
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Server.RoundEnded -= OnRoundEnded;
            Exiled.Events.Handlers.Player.Destroying -= OnDestroying;
            Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;

            // This will prevent commands and other classes from being able to access
            // any state while the plugin is disabled
            Singleton = null;

            base.OnDisabled();
        }

        public void OnRoundStart()
        {
            ScpsAwaitingReplacement.Clear();
            playerDataMap.Clear();
            coroutine = Timing.RunCoroutine(timer());
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Timing.KillCoroutines(coroutine);
        }

        private IEnumerator<float> timer()
        {
            while (true)
            {
                try
                {
                    foreach (Player player in Player.List)
                    {
                        if (player != null
                            && player.IsAlive
                            && player.Rotation != null
                            && player.Position != null
                            && player.Role.Type != RoleTypeId.Scp079
                            // skip staff, unless staff are not immune
                            && (!player.RemoteAdminAccess || !Config.StaffAfkImmune))
                        {
                            PlayerData playerData;
                            var hasData = playerDataMap.TryGetValue(player, out playerData);
                            if (!hasData)
                            {
                                playerDataMap[player] = new PlayerData();
                                playerData = playerDataMap[player];
                            }

                            //Log.Info($"{player.Nickname} old rot: {playerData.LastPlayerRotation} new rot: {player.Rotation} old pos: {playerData.LastPlayerPosition}  new pos: {player.Position} seconds: {playerData.SecondsSinceRotationChange} despawns: {playerData.DespawnCount}");
                            var oldRotation = playerData.LastPlayerRotation;
                            var newRotation = player.Rotation;

                            var oldPosition = playerData.LastPlayerPosition;
                            var newPosition = player.Position;

                            // if not afk, i.e. is moving
                            if (oldRotation == null || oldRotation != newRotation || oldPosition == null || oldPosition != newPosition)
                            {
                                playerData.SecondsSinceRotationChange = 0;
                            }
                            else // if not moving
                            {
                                var secondsUntilKickOrDespawn = Config.SecondsBeforeAFKKickOrDespawn - playerData.SecondsSinceRotationChange;
                                if (playerData.SecondsSinceRotationChange >= Config.SecondsBeforeAFKWarn)
                                {
                                    var s = secondsUntilKickOrDespawn == 1 ? "" : "s";

                                    var action = playerData.DespawnCount >= Config.DespawnsBeforeKick ? "kicked" : "despawned";

                                    player.Broadcast(1,
                                        $"<color=red>Warning:\n</color><color=yellow>You will be automatically AFK {action} in <color=red>{secondsUntilKickOrDespawn} second{s}</color>.\n<color=green>Please move to reset the AFK timer.</color>",
                                        Broadcast.BroadcastFlags.Normal,
                                        true
                                        );
                                }
                                if (secondsUntilKickOrDespawn <= 0)
                                {
                                    if (playerData.DespawnCount >= Config.DespawnsBeforeKick)
                                    {
                                        ReplacePlayer(player);
                                        Timing.CallDelayed(0.5f, () =>
                                        {
                                            player.Kick("You were automatically AFK kicked by a plugin. Press the Re-Join button to re-connect.");
                                        });
                                    }
                                    else
                                    {
                                        player.DespawnAndReplace();
                                        playerData.DespawnCount++;
                                    }
                                }

                                playerData.SecondsSinceRotationChange++;
                            }
                            playerData.LastPlayerRotation = newRotation;
                            playerData.LastPlayerPosition = newPosition;
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        private void OnDestroying(DestroyingEventArgs ev)
        {
            if (ev.Player == null)
                return;
            playerDataMap.Remove(ev.Player);

            var playerToReplace = ev.Player;
            var elapsedSeconds = Round.ElapsedTime.TotalSeconds;

            // only replace quitting SCPs who quit at the start of the game
            if (playerToReplace.IsScp && playerToReplace.Role != RoleTypeId.Scp0492 && elapsedSeconds < Config.QuitCutoff)
            {
                var requiredHealth = (int)(Config.RequiredHealthPercentage / 100.0 * playerToReplace.MaxHealth);
                var scpNumber = playerToReplace.Role.ScpNumber();
                Log.Info($"{playerToReplace.Nickname} left {elapsedSeconds} seconds into the round, was SCP-{scpNumber} with {playerToReplace.Health}/{playerToReplace.MaxHealth} HP ({requiredHealth} required for replacement)");

                if (playerToReplace.Health < requiredHealth)
                {
                    Log.Info("This SCP will not be replaced because they have lost too much health");
                    return;
                }

                // Let all non-SCPs (including spectators) know of the opportunity to become SCP
                // SCPs are not told of this because then they would also have to be replaced after swapping 
                foreach (var p in Player.List.Where(x => !x.IsScp))
                {
                    var message = Translation.ReplaceBroadcast.Replace("%NUMBER%", scpNumber);
                    // Longer broadcast time since beta test revealed users were having trouble reading it all in time
                    p.Broadcast(16, Translation.BroadcastHeader + message);
                    // Also send conole message in case they miss the broadcast
                    p.SendConsoleMessage(message, "yellow");
                }

                // Add the SCP to our list so that a user can claim it with the .volunteer command
                ScpsAwaitingReplacement.Add(playerToReplace.Role);
            }
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Player != null && playerDataMap != null && playerDataMap.TryGetValue(ev.Player, out PlayerData data))
                data.SecondsSinceRotationChange = 0;
        }


        public void ReplacePlayer(Player playerToReplace)
        {
            if (playerToReplace == null || playerToReplace.Position == null || !playerToReplace.IsAlive)
                return;

            var position = playerToReplace.Position;
            if (playerToReplace.IsInElevator())
                return;

            // regular spectator replacement
            var spectators = Player.List.Where(p => p.Role.Type == RoleTypeId.Spectator);
            if (spectators == null || spectators.IsEmpty())
                return;

            var spectator = spectators.RandomElement();

            spectator.Role.Set(playerToReplace.Role.Type, SpawnReason.Respawn);
            var roleName = $"<color={playerToReplace.Role.Color.ToHex()}>{playerToReplace.Role.Type.GetFullName()}</color>";
            foreach (CustomRole role in playerToReplace.GetCustomRoles())
            {
                role.AddRole(spectator);
                roleName = $"<color={playerToReplace.Role.Color.ToHex()}>{role.Name}</color>";
            }

            var playerHealth = playerToReplace.Health;
            var playerMaxHealth = playerToReplace.MaxHealth;
            var playerArtificialHealth = playerToReplace.ArtificialHealth;
            var playerMaxArtificialHealth = playerToReplace.MaxArtificialHealth;
            var playerHumeShield = playerToReplace.HumeShield;
            var playerCuffer = playerToReplace.Cuffer;
            Log.Info("a");

            spectator.Broadcast(10, $"<color=yellow>You have replaced an AFK player and become <color={playerToReplace.Role.Color.ToHex()}>{roleName}</color>.</color>\nItems are on the ground.");
            Timing.CallDelayed(1, () =>
            {
                Log.Info("b");
                spectator.Teleport(position);
                spectator.Health = playerHealth;
                spectator.MaxHealth = playerMaxHealth;
                spectator.ArtificialHealth = playerArtificialHealth;
                spectator.MaxArtificialHealth = playerMaxArtificialHealth;
                spectator.HumeShield = playerHumeShield;
                spectator.Cuffer = playerCuffer;
            });

        }

        /// <summary>
        ///     Whether the replacement cutoff (i.e. the max time in seconds after the round
        ///     that a player can still use .volunteer) has passed
        ///     We put this function here so that we can conveniently access the Config without
        ///     needing to implement the Singleton pattern in Config
        /// </summary>
        /// <returns>whether the replacement period cutoff has passed (true if passed)</returns>
        public bool HasReplacementCutoffPassed()
        {
            return Round.ElapsedTime.TotalSeconds > Config.ReplaceCutoff;
        }
    }
}