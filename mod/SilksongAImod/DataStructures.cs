using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace SilksongAI
{
    [MessagePackObject]
    public struct FrameData
    {
        [Key(0)]
        public int frame;
        [Key(1)]
        public EnemyData enemy;
        [Key(2)]
        public HeroData hero;

    }

    [MessagePackObject]
    public struct EnemyData
    {
        [Key(0)]
        public Vector3 pos;
        [Key(1)]
        public Vector2 vel;
        [Key(2)]
        public int hp;

    }
    [MessagePackObject]
    public struct HeroData
    {
        [Key(0)]
        public Vector3 pos;
        [Key(1)]
        public Vector3 vel;
        [Key(2)]
        public int hp;
    }


}
