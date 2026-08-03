using System;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Orchestration.Interfaces;
namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionAssembler : IBossfightSessionAssembler 
    {
        private readonly IBossfightSessionObjectFactory _objectFactory;
        private readonly BossfightSessionTypeRegistry _sessionTypes;
        public BossfightSessionTypeRegistry SessionTypes => _sessionTypes;
        public BossfightSessionAssembler(
            IBossfightSessionObjectFactory objectFactory,
            BossfightSessionTypeRegistry sessionTypes)
        {
            _objectFactory = objectFactory;
            _sessionTypes = sessionTypes;
        }
        public BossfightSession Assemble(string type, BossfightSessionConfiguration config)
        {
            var boss = config.Boss;

            var builder = new BossfightSessionBuilder(config.Boss)
                .UseLoadout(config.Loadout)
                .UseBoundary(config.Boundary);

            if (!_sessionTypes.TryCreate(type, config, out var plugins))
                throw new ArgumentException($"There is no Session Type '{type}'");

            foreach (var plugin in plugins)
            {
                builder.AddPlugin(plugin);
            }

            IRespawnPoint respawnPoint;
            if (boss.CanRespawnOnArena)
            {
                respawnPoint = _objectFactory.CreateRespawnPoint(boss.ArenaSceneName, boss.ArenaPosition, "BossfightSession respawn point");
            }
            else
            {
                respawnPoint = _objectFactory.CreateDefaultRespawnPoint();
            }
            builder.UseRespawnPoint(respawnPoint);

            return builder.Build();
        }
    }
}
