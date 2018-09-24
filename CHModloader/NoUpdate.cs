using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CHModloader
{
    class NoUpdate : Mod
    {
        public override string ID { get { return "NoUpdate"; } }
        public override string Name { get { return "No CH Update"; } }
        public override string Version { get { return ModLoader.Version; } }
        public override string Author { get { return "Jacon"; } }
    }
}
