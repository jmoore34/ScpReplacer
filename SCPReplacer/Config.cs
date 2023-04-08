using Exiled.API.Interfaces;
using System.ComponentModel;

namespace SCPReplacer
{
    public class Config : IConfig
    {
        public bool Debug { get; set; } = false;

        [Description("How many seconds a player can be AFK before they start getting warned about an imminent AFK kick")]
        public int SecondsBeforeAFKWarn { get; set; } = 40;

        [Description("How many seconds a player can be AFK before being kicked")]
        public int SecondsBeforeAFKKickOrDespawn { get; set; } = 50;

        [Description("Whether players with RA access are immune to being automatically AFK despawned/kicked")]
        public bool StaffAfkImmune { get; set; } = true;

        [Description("How many times a player should be AFK Despawned before being AFK Kicked")]
        public int DespawnsBeforeKick { get; set; } = 1;

        [Description("The maximum time after the round start, in seconds, that a quitting SCP can cause the volunteer opportunity announcement (defaults to 60)")]
        public int QuitCutoff { get; set; } = 60;

        [Description("The maximum time after the round start, in seconds, that a player can use the .volunteer command (defaults to 85)")]
        public int ReplaceCutoff { get; set; } = 85;

        [Description("The required percentage of health (0-100) the SCP must have had to be eligible for replacement. Defaults to 95 (no percent sign)")]
        public int RequiredHealthPercentage { get; set; } = 95;

        [Description("Is the plugin enabled?")]
        public bool IsEnabled { get; set; } = true;
    }
}