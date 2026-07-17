using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface ILoadoutManager //TODO consider wether such interface is even needed as it will likely only be used in .Mod
    {
        Loadout BuildLoadout(string name);
        void SetLoadout(Loadout Loadout);
        void SetLoadoutTemporary(Loadout Loadout, TemporaryStateModifier modifier);
    }
}
