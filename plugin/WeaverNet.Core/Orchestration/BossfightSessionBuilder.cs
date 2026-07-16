using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Orchestration.Boundaries;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionBuilder
    {
        private BossData Boss { get; }
        private IBossfightSessionBoundary Boundary { get; }
        private Loadout Loadout { get; set; }
        private IReadOnlyCollection<IBossfightSessionPlugin> Plugins { get; set; }
        private IRespawnPoint RespawnPoint { get; set; }
        public BossfightSessionBuilder(
            BossData boss, 
            IBossfightSessionBoundary boundary, 
            ILoadoutManager loadoutManager,
            IRespawnPointFactory rpf)
        {
            Boss = boss;
            Boundary = boundary;
            Loadout = loadoutManager.GetLoadout();
            Plugins = new List<IBossfightSessionPlugin>();

            if (Boss.CanRespawnOnArena)
            {
                RespawnPoint = rpf.RespawnPoint(Boss.ArenaSceneName, Boss.ArenaPosition); // TODO we have to deal with disposing this somehow if for example one calls UseRespawnPoint the old game object will remain in game forever
            }
            else
            {
                RespawnPoint = rpf.DefaultRespawnPoint();
            }
        }
        public BossfightSessionBuilder UseRespawnPoint(IRespawnPoint respawnPoint)
        {
            RespawnPoint = respawnPoint;
            return this;
        }
        public BossfightSessionBuilder UseLoadout(Loadout loadout)
        {
            Loadout = loadout;
            return this;
        }

        public BossfightSession Build()
        {
            return new BossfightSession(
                Boss,
                Loadout,
                Boundary,
                Plugins,
                RespawnPoint
            );
        }
    }
}
