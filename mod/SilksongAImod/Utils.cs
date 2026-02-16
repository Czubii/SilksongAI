using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace SilksongAI
{
    public static class TeleportUtils
    {

        private static Vector3 pendingPos;
        private static string pendingScene;
        private static bool pendingTeleport;
        public static void TeleportTo(string sceneName, Vector3 pos)
        {
            pendingScene = sceneName;
            pendingPos = pos;
            pendingTeleport = true;

            GameManager.instance.ChangeToScene(sceneName, "top1", 0.5f);
        }

        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!pendingTeleport) return;
            if (scene.name != pendingScene) return;

            pendingTeleport = false;

            HeroController.instance.transform.position = pendingPos;
            GameManager.instance.cameraCtrl.PositionToHeroInstant(false);
        }
    }

    public static class GetDataUtils
    {
        private static EnemyData? getEnemyData(string enemyName)
        {
            var enemy = GameObject.Find(enemyName);
            if (enemy == null) return null;

            var hm = enemy.GetComponent<HealthManager>();
            if (hm == null) return null;

            var fsm = enemy.GetComponent<PlayMakerFSM>();
            if (fsm == null) return null;

            var rigidbody = enemy.GetComponent<Rigidbody2D>();
            if (rigidbody == null) return null;

            EnemyData output = new EnemyData();

            output.pos = enemy.transform.position;
            output.vel = rigidbody.linearVelocity;
            output.hp = hm.hp;
            output.fsm = fsm;


            return output;
        }

        private static Vector3 GetEnemyPosition(string enemyName)
        {
            var enemy = GameObject.Find(enemyName);
            if (enemy != null)
            {
                return enemy.transform.position;
            }
            return Vector3.zero;
        }


        private static void GetGameObjectComponents(string gameObjectName)
        {
            var obj = GameObject.Find(gameObjectName);
            if (obj == null) return;
            int numComponentes = obj.GetComponentCount();
            SilksongAImod.Log.LogInfo($"Num components: {numComponentes}");
            for (int i = 0; i < numComponentes; i++)
                SilksongAImod.Log.LogInfo(obj.GetComponentAtIndex(i));
        }

        private static PlayMakerFSM[] GetEnemyFSM(string enemyName)
        {
            var enemy = GameObject.Find(enemyName);
            if (enemy != null)
            {
                var fsms = enemy.GetComponents<PlayMakerFSM>();
                return fsms;
            }

            return null;

        }

        private static int GetEnemyHP(string enemyName)
        {
            int hp = -1;

            var enemy = GameObject.Find(enemyName);
            if (enemy != null)
            {
                var hm = enemy.GetComponent<HealthManager>();
                hp = hm.hp;
            }

            return hp;
        }
    }

}
