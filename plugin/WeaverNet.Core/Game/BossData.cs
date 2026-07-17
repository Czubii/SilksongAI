using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Core.Game
{

    public class BossData
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string ArenaSceneName { get; }
        public Vector3 ArenaPosition { get; }
        public bool CanRespawnOnArena { get; }
        public bool RequireHardSceneReload { get; }
        public BossRespawnFlags RespawnFlags { get; }

        public BossData(
            string id,
            string displayName,
            string arenaSceneName,
            Vector3 arenaPosition,
            BossRespawnFlags respawnFlags,
            bool canRespawnOnArena,
            bool requireHardSceneReload)
        {
            Id = id;
            DisplayName = displayName;
            ArenaSceneName = arenaSceneName;
            ArenaPosition = arenaPosition;
            RespawnFlags = respawnFlags;
            CanRespawnOnArena = canRespawnOnArena;
            RequireHardSceneReload = requireHardSceneReload;
        }
    }
}
