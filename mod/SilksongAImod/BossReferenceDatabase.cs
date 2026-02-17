using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SilksongAI
{

    public struct BossReference
    {
        public string Name;
        public string ArenaMap;
        public Vector3 ArenaPosition;
        public Action<bool> SetDefeated;
    }
    public static class BossReferenceDatabase
    {

        public static readonly List<BossReference> All = new List<BossReference>
        {
            new BossReference
            {
                Name = "Mossbone Mother",
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
                Name = "Bell Beast",
                ArenaMap = "Bone_05",
                ArenaPosition = new Vector3(78.59f, 3.57f, 0),
                SetDefeated = (val) =>
                {
                    PlayerData.instance.defeatedBellBeast = val;
                }
            },
            new BossReference
            {
                Name = "Lace 1",
                ArenaMap = "Tut_03",
                ArenaPosition = new Vector3(68f, 17.6f, 0)
            },

        };
    }
}
