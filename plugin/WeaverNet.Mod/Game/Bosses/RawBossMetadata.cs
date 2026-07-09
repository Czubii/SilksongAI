using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Mod.Game.Bosses
{
    public class RawBossMetadata
    {
        public string ID { get; set; }
        public string DisplayName { get; set; }
        public string ArenaSceneName { get; set; }
        public Vector3 ArenaPosition { get; set; }
        public bool CanRespawnOnArena { get; set; }
        public bool RequireHardSceneReload { get; set; }
        public AbilitySet Abilities { get; set; }
        public ICrestToolSet Tools { get; set; }
        public RespawnFlags RespawnFlags { get; set; }

        public BossMetadata Build()
        {
            return new BossMetadata
            {
                ID = ID,
                DisplayName = DisplayName,
                ArenaSceneName = ArenaSceneName,
                ArenaPosition = ArenaPosition,
                CanRespawnOnArena = CanRespawnOnArena,
                RequireHardSceneReload = RequireHardSceneReload,
                Abilities = Abilities,
                Tools = Tools,
                Behavior = new DataDrivenBossBehavior(RespawnFlags)
            };
        }
    }

    public class RespawnFlags
    {
        // Using a Dictionary because the keys change depending on the boss
        public Dictionary<string, bool> PlayerDataBools { get; set; }
        public List<SceneBool> SceneBools { get; set; }
        public List<SceneInt> SceneInts { get; set; }
    }

    public class SceneBool
    {
        public string SceneName { get; set; }
        public string ID { get; set; }
        public bool Value { get; set; }
    }

    public class SceneInt
    {
        public string SceneName { get; set; }
        public string ID { get; set; }
        public int Value { get; set; }
        public int Mutator { get; set; }
    }
}
