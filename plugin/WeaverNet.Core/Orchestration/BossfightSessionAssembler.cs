using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    // How to create, store and wire services and plugins together
    // 
    // For mod-lifetime services:
    // - a registry containing all of the services with get method
    // - those services prefferably should not contain any internal state to avoid runtime corruption and errors
    // - an example of such service would be the teleport service, although i think it will not be needed here
    //
    // For session-lifetime services (Plguins):
    // - a Plugin factory class responsible for instantiating the provided classes via dependency injection
    // - a registry class reposnisble for storing, and providing plugins. this allows to avoid duplicate clasess (for example for recording
    //   and ML inference a single frame capture service can be used which passes the frames further for saving/inference). It would also help manage the lifetime of the services
    //   as it can dispose all of them when going out of scope 

    public class BossfightSessionAssembler : IBossfightSessionAssembler
    {
        private readonly BossfightSessionObjectFactory _objectFactory;
        public BossfightSessionAssembler(BossfightSessionObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }
        public BossfightSession Assemble(BossfightSessionConfiguration config)
        {
            var boss = config.Boss;

            var registry = new ObjectRegistry();
            var builder = new BossfightSessionBuilder(config.Boss)
                .UseLoadout(config.Loadout)
                .UseBoundary(config.Boundary);

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
