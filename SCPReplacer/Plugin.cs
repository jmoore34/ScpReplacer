using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPReplacer
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "SCP Replacer";
        public override string Author => "Jon M";
        public override Version Version => new Version(1, 0, 0);

        public override void OnEnabled()
        {
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            base.OnDisabled(); 
        }
    }
}
