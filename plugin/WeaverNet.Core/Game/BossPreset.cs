using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Game
{
    public class BossPreset
    {
        public BossData Boss { get; }
        public Loadout Loadout { get; }
        public BossPreset(BossData definition, Loadout loadout)
        {
            Boss = definition;
            Loadout = loadout;
        }
    }
}
