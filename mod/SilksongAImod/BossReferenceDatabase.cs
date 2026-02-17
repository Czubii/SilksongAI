using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SilksongAI
{

    public class BossReference
    {
        public string DisplayName;
        public string InternalName;
        public string ArenaMap;
        public Vector3 ArenaPosition;
        public Action<bool> SetDefeated = _ => SilksongAImod.Log.LogWarning("Trying to respawn a boss with undefined SetDefeated action!");
    }
    public static class BossReferenceDatabase
    {

        public static readonly List<BossReference> All = new List<BossReference>
        {
            new BossReference
            {
                DisplayName = "Moss Mother",
                InternalName = "Mossbone Mother",
                ArenaMap = "Tut_03",
                ArenaPosition = new Vector3(68f, 17.6f, 0),
                SetDefeated = (val) => 
                {
                    PlayerData.instance.defeatedMossMother = val;
                    SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
                    {
                        SceneName = "Tut_03",
                        ID = "Battle Scene",
                        Value = val
                    });
                }
            },
            new BossReference
            {
                DisplayName = "Bell Beast",
                InternalName = "Bell Beast",
                ArenaMap = "Bone_05",
                ArenaPosition = new Vector3(78.59f, 3.57f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedBellBeast = val;
                }
            },
            new BossReference
            {
                DisplayName = "Lace 1",
                InternalName = "Lace Boss1",
                ArenaMap = "Bone_East_12",
                ArenaPosition = new Vector3(85f, 7.57f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedLace1 = val;
                    PlayerData.instance.encounteredLace1 = val;
                }
            },

        };
    }
}
