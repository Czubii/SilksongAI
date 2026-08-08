using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    /// <summary>
    /// provides 
    /// </summary>
    public interface IBossfightCatalog
    {
        IReadOnlyList<BossData> Bosses { get; }
        IReadOnlyList<Loadout> Loadouts { get; }
    }
}
