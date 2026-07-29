using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface ILoadoutQuery
    {
        Loadout BuildLoadout(string name);
    }
}
