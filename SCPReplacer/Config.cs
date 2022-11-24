using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Interfaces;

namespace SCPReplacer
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        [Description("The maximum time after the round start, in seconds, that a quitting SCP can cause the volunteer opportunity announcement")]
        public int QuitCutoff = 60;
        [Description("The maximum time after the round start, in seconds, that a player can use the .volunteer command")]
        public int ReplaceCutoff = 85;
        [Description("The required percentage of health (0-100) the SCP must have had to be eligible for replacement. Defaults to 95 (no percent sign)")]
        public int RequiredHealthPercentage = 95;
        [Description("Whether to use Chaos Theory branding in broadcasts")]
        public bool ChaosTheoryBranding = true;
    }
}
