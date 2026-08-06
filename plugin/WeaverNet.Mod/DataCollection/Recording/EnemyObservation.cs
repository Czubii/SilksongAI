using System.Collections.Generic;

namespace WeaverNet.Mod.DataCollection.Recording
{
    public class EnemyObservation
    {
        public string EnemyInternalName { get; }
        public int Hp { get; }
        public float PosX { get; }
        public float PosY { get; }

        public bool FacingRight;
        public float VelX { get; } // in processing we can make this a relative velocity
        public float VelY { get; } // in processing we can make this a relative velocity

        public List<FsmObservation> PlayMakers; //PlayMakerFSM.FsmName and PlayMakerFSM.ActiveStateName

        public EnemyObservation(
            string enemyInternalName, 
            int hp, 
            float posX, float posY, 
            bool facingRight, 
            float velX, float velY, 
            List<FsmObservation> playMakers)
        {
            EnemyInternalName = enemyInternalName;
            Hp = hp;
            PosX = posX;
            PosY = posY;
            FacingRight = facingRight;
            VelX = velX;
            VelY = velY;
            PlayMakers = playMakers;
        }
    }
}
