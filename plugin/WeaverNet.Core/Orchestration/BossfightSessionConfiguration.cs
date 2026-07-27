using System.Collections.Generic;
using WeaverNet.Core.Game;
using WeaverNet.Core.Orchestration.Boundaries;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionConfiguration
    {
        public BossData Boss { get; }
        public Loadout Loadout { get; }
        public IBossfightSessionBoundary Boundary { get; }
        public IReadOnlyCollection<BossfightSessionOption> Features { get; }
        public BossfightSessionConfiguration(BossData boss, Loadout loadout, IBossfightSessionBoundary boundary, IReadOnlyCollection<BossfightSessionOption> features)
        {
            Boss = boss;
            Loadout = loadout;
            Boundary = boundary;
            Features = features;
        }
    }
}
