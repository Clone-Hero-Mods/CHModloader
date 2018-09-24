using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CHModloader;

namespace SampleMod
{
    public class SampleMod : Mod
    {
        public override string ID => "SampleMod";
        public override string Name => "Sample Mod";
        public override string Version => "1.0";
        public override string Author => "Sample Author";
        public override bool HasTextures => true;

        public override void OnInit()
        {
            ModLogs.Log("Hello from sample mod!");
        }
    }
}
