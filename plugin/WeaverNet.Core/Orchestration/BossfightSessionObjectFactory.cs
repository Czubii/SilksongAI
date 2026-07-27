
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionObjectFactory
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
