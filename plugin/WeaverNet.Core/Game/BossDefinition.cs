using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Core.Game
{

    public class BossDefinition
    {
        public string ID { get; }
        public string DisplayName { get; }
        public string ArenaSceneName { get; }
        public Vector3 ArenaPosition { get; }
        public bool CanRespawnOnArena { get; }
        public bool RequireHardSceneReload { get; }
        public IBossSpawner Spawner { get; }

        public BossDefinition(
            string id,
            string displayName,
            string arenaSceneName,
            Vector3 arenaPosition,
            IBossSpawner spawner,
            bool canRespawnOnArena = true,
            bool requireHardSceneReload = false)
        {
            ID = id;
            DisplayName = displayName;
            ArenaSceneName = arenaSceneName;
            ArenaPosition = arenaPosition;
            Spawner = spawner;
            CanRespawnOnArena = canRespawnOnArena;
            RequireHardSceneReload = requireHardSceneReload;
        }
    }
}
