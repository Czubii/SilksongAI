using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;

namespace WeaverNet.Core.Infrastructure.Interfaces
{
    /// <summary>
    /// provides a catalog of values that can be used to filter recordings, such as distinct bosses that have been recorded.
    /// auto updates when the underlying recording repository changes.
    /// </summary>
    public interface IRecordingCatalog
    {
        IReadOnlyList<BossData> DistinctBosses { get; }
        IReadOnlyList<Loadout> DistinctLoadouts { get; }
    }
}
