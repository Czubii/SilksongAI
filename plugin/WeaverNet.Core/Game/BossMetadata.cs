using UnityEngine;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Core.Game
{
    public class BossMetadata
    {
        public string DisplayName;
        public string ID;
        public string ArenaSceneName;
        public Vector3 ArenaPosition;
        public bool CanRespawnOnArena = true; // TODO: implement (for example false for fouth chorus)
        public bool RequireHardSceneReload = false;
        public IBossBehavior behavior;
    }
}
