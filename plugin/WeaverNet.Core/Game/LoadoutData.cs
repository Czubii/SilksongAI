using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Game
{
    public class LoadoutData
    {
        public string Name { get; }
        public Loadout Loadout { get; }
        public LoadoutData(string name, Loadout loadout)
        {
            Name = name;
            Loadout = loadout;
        }
    }
}
