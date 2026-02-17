using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SilksongAI
{

    public struct BossInfo
    {
        public string Name;
        public string ArenaMap;
        public Vector3 ArenaPosition;
    }
    public static class BossDatabase
    {

        public static readonly List<BossInfo> All = new List<BossInfo>
        {
            new BossInfo
            {
                Name = "Mossbone Mother",
                ArenaMap = "Tut_03",
                ArenaPosition = new Vector3(68f, 17.6f, 0)
            },
            new BossInfo
            {
                Name = "Bell Beast",
                ArenaMap = "Bone_05",
                ArenaPosition = new Vector3(78.59f, 3.57f, 0)
            },
            new BossInfo
            {
                Name = "Lace 1",
                ArenaMap = "Tut_03",
                ArenaPosition = new Vector3(68f, 17.6f, 0)
            },

        };
    }
}
