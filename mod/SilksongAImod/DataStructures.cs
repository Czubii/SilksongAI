using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace SilksongAI
{
    public struct EnemyData
    {

        public Vector3 pos;
        public Vector2 vel;
        public int hp;
        public PlayMakerFSM fsm;

    }

    public struct HeroData
    {
        public Vector3 pos;
        public Vector3 vel;
        public int hp;
    }


}
