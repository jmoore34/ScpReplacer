using System.ComponentModel;
using Exiled.API.Interfaces;

namespace SCPReplacer
{
    public class Config : IConfig
    {
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