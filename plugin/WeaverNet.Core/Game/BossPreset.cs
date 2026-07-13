using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Game
{
    public class BossPreset
    {
        public readonly BossDefinition Definition;
        public readonly Loadout Loadout;

        public BossPreset(BossDefinition definition, Loadout loadout)
        {
            Definition = definition;
            Loadout = loadout;
        }
    }
}
