using System;
using System.Collections.Generic;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Orchestration.Boundaries;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSession
    {
        public BossData Boss { get; }
        public Loadout Loadout { get; }
        public IBossfightSessionBoundary Boundary { get; }
        public IReadOnlyCollection<IBossfightSessionPlugin> Plugins { get; }
        public IRespawnPoint RespawnPoint { get; }

        public BossfightSession(
            BossData boss, 
            Loadout loadout, 
            IBossfightSessionBoundary boundary, 
            IReadOnlyCollection<IBossfightSessionPlugin> plugins,
            IRespawnPoint respawnPoint)
        {
            Boss = boss;
            Loadout = loadout;
            Boundary = boundary;
            Plugins = plugins;
            RespawnPoint = respawnPoint;
        }
    }
}
