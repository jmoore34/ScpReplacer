using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Exiled.API.Features.Roles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace SCPReplacer
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCP Replacer";
        public override string Author => "Jon M";
        public override Version Version => new Version(1, 0, 0);

        public override void OnEnabled()
        {
            Instance = this;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeave;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeave;
            base.OnDisabled(); 
        }

        public static Plugin Instance { get; private set; } = null;


        public List<Exiled.API.Features.Roles.Role> ScpsAwaitingReplacement { get; private set; } = new List<Exiled.API.Features.Roles.Role>();
        public Stopwatch RoundTimer { get; private set; } = new Stopwatch(); 

        public void OnRoundStart()
        {
            ScpsAwaitingReplacement.Clear();
            RoundTimer.Restart();
        }

        public void OnPlayerLeave(LeftEventArgs ev)
        {
            if (ev.Player.Role == RoleType.Scp079 || ev.Player.Role == RoleType.Scp106 || ev.Player.Role == RoleType.Scp173 || ev.Player.Role == RoleType.Scp096)
            {
                long elapsedSeconds = RoundTimer.ElapsedMilliseconds / 1000;
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
                foreach (Exiled.API.Features.Player p in Exiled.API.Features.Player.List.Where(x => !x.IsScp))
                {
                    p.Broadcast(new Exiled.API.Features.Broadcast(
                        $"{BroadcastHeader()}Enter <color=green>.volunteer {scpNumber}</color> in the <color=orange>~</color> console to become <color=red>SCP-{scpNumber}</color>"));
                }

                ScpsAwaitingReplacement.Add(ev.Player.Role);
            }
        }

        public bool HasReplacementCutoffPassed() => RoundTimer.ElapsedMilliseconds / 1000 > Config.ReplaceCutoff;
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
