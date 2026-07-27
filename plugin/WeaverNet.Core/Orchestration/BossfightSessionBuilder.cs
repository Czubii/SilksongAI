using JetBrains.Annotations;
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
    internal class BossfightSessionBuilder
    {
        private readonly BossData _boss;
        private IBossfightSessionBoundary _boundary;
        private Loadout _loadout;
        private readonly List<IBossfightSessionPlugin> _plugins = new List<IBossfightSessionPlugin>();
        private IRespawnPoint _respawnPoint;
        public BossfightSessionBuilder(BossData boss)
        {
            _boss = boss;
        }
        public BossfightSessionBuilder UseBoundary(
            IBossfightSessionBoundary boundary)
        {
            _boundary = boundary;
            return this;
        }
        public BossfightSessionBuilder UseLoadout(
            Loadout loadout)
        {
            _loadout = loadout;
            return this;
        }
        public BossfightSessionBuilder UseRespawnPoint(IRespawnPoint respawnPoint)
        {
            _respawnPoint = respawnPoint;
            return this;    
        }
        public BossfightSessionBuilder AddPlugin(
            IBossfightSessionPlugin plugin)
        {
            _plugins.Add(plugin);
            return this;
        }
        public BossfightSession Build()
        {
            return new BossfightSession(
                _boss,
                _loadout,
                _boundary,
                _plugins,
                _respawnPoint);
        }
    }
}
