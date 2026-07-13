using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.Game;

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
        public CrestToolSet Tools { get; set; }
        public RespawnFlags RespawnFlags { get; set; }

        public BossPreset Build()
        {
            return new BossPreset
            (
                new BossDefinition
                (
                    ID,
                    DisplayName,
                    ArenaSceneName,
                    ArenaPosition,
                    new DataDrivenBossSpawner(RespawnFlags),
                    CanRespawnOnArena,
                    RequireHardSceneReload
                ),
                new Loadout
                (
                    Abilities,
                    Tools
                )
            );
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
