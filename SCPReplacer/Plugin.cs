using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.API.Features.Roles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets._Scripts.Dissonance;

namespace SCPReplacer
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCP Replacer";
        public override string Author => "Jon M";
        public override Version Version => new Version(1, 0, 0);

        public override void OnEnabled()
        {
            // Set up the Singleton so we can easily get the instance with all the state
            // from another class.
            Instance = this;
            // Register event handlers
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeave;

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            // This will prevent commands and other classes from being able to access
            // any state while the plugin is disabled
            Instance = null;
            // Deregister event handlers
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;

            base.OnDisabled(); 
        }

        // Singleton pattern allows easy access to the central state from other classes
        // (e.g. commands)
        public static Plugin Instance { get; private set; } = null;

        // A list of Roles (SCPs) that have quit early in the round and have not yet been replaced
        public List<Exiled.API.Features.Roles.Role> ScpsAwaitingReplacement { get; private set; } = new List<Exiled.API.Features.Roles.Role>();
        // A timer is used to keep track of how much time has passed since the beginning of the round
        public Stopwatch RoundTimer { get; private set; } = new Stopwatch(); 

        public void OnRoundStart()
        {
            ScpsAwaitingReplacement.Clear();
            RoundTimer.Restart();
        }

        public void OnRoundEnd()
        {
            // Prevent any swapping after the round ends (in the edge case of a round
            // that ends super early)
            ScpsAwaitingReplacement.Clear();
        }

        public void OnPlayerLeave(LeftEventArgs ev)
        {
            if (ev.Player.IsScp && ev.Player.Role != RoleType.Scp0492)
            {
                long elapsedSeconds = RoundTimer.ElapsedMilliseconds / 1000;
                // Minimum required health (configurable percentage) of the SCP
                // when they quit to be eligible for replacement
                int requiredHealth = (int)(Config.RequiredHealthPercentage / 100.0 * ev.Player.MaxHealth);
                string scpNumber = ev.Player.Role.ScpNumber();
                Log.Info($"{ev.Player.Nickname} left {elapsedSeconds} seconds into the round, was SCP-{scpNumber} with {ev.Player.Health}/{ev.Player.MaxHealth} HP ({requiredHealth} required for replacement)");
                if (elapsedSeconds > Config.QuitCutoff)
                {
                    Log.Info("This SCP will not be replaced because the quit cutoff has already passsed");
                    return;
                }
                if (ev.Player.Health < requiredHealth)
                {
                    Log.Info("This SCP will not be replaced because they have lost too much health");
                    return;
                }
                // Let all non-SCPs (including spectators) know of the opportunity to become SCP
                // SCPs are not told of this because then they would also have to be replaced after swapping 
                foreach (Exiled.API.Features.Player p in Exiled.API.Features.Player.List.Where(x => !x.IsScp))
                {
                    p.Broadcast(new Exiled.API.Features.Broadcast(
                        $"{BroadcastHeader()}Enter <color=green>.volunteer {scpNumber}</color> in the <color=orange>~</color> console to become <color=red>SCP-{scpNumber}</color>"));
                }
                // Add the SCP to our list so that a user can claim it with the .volunteer command
                ScpsAwaitingReplacement.Add(ev.Player.Role);
            }
        }

        /// <summary>
        /// Whether the replacement cutoff (i.e. the max time in seconds after the round
        /// that a player can still use .volunteer) has passed
        /// 
        /// We put this function here so that we can conveniently access the Config without
        /// needing to implement the Singleton pattern in Config
        /// </summary>
        /// <returns>whether the replacement period cutoff has passed (true if passed)</returns>
        public bool HasReplacementCutoffPassed() => RoundTimer.ElapsedMilliseconds / 1000 > Config.ReplaceCutoff;

        /// <summary>
        /// Returns a string that we should prefix broadcasts with
        /// 
        /// This function exists within this class so the Config can be conveniently accessed
        /// (and thus we can decide whether to include the Chaos Theory branding in the broadcast header)
        /// </summary>
        /// <returns>a string with color tags ending in a newline, to be inserted before every broadcast</returns>
        public string BroadcastHeader()
        {
            if (Config.ChaosTheoryBranding)
            {
                return "<color=yellow>[Chaos Theory SCP Replacer]</color>\n";
            } else
            {
                return "<color=yellow>[SCP Replacer]</color>\n";
            }
        }
    }
}
