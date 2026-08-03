
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Mod.DataCollection
{
    public class BossfightSessionObjectFactory : IBossfightSessionObjectFactory
    {
        private readonly IRespawnPointFactory _respawnPointFactory;
        public BossfightSessionObjectFactory(IRespawnPointFactory respawnPointFactory)
        {
            _respawnPointFactory = respawnPointFactory;
        }
        public IRespawnPoint CreateRespawnPoint(string scene, Vector3 position, string name) =>
            _respawnPointFactory.RespawnPoint(scene, position, name);
        public IRespawnPoint CreateDefaultRespawnPoint() => _respawnPointFactory.DefaultRespawnPoint();
    }
}
