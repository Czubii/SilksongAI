using UnityEngine;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Core.Game
{
    public class BossMetadata
    {
        public string ID;
        public string DisplayName;
        public string ArenaSceneName;
        public Vector3 ArenaPosition;
        public bool CanRespawnOnArena = true;
        public bool RequireHardSceneReload = false;
        public IAbilitySet Abilities;
        public ICrestToolSet Tools;
        public IBossBehavior Behavior;
    }
}
