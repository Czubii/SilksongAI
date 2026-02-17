using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using HutongGames.PlayMaker.Actions;

namespace SilksongAI
{
    public static class TeleportUtils
    {

        private static Vector3 pendingPos;
        private static string pendingScene;
        private static bool pendingTeleport;
        public static void TeleportTo(string sceneName, Vector3 pos)
        {
            if (pendingTeleport) return;

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
        public static EnemyData? getEnemyData(GameObject enemy)
        {

            var hm = enemy.GetComponent<HealthManager>();
            if (hm == null) return null;

            var fsm = enemy.GetComponent<PlayMakerFSM>();
            if (fsm == null) return null;

            var rigidbody = enemy.GetComponent<Rigidbody2D>();
            if (rigidbody == null) return null;

            EnemyData output = new EnemyData
            {

                posX = enemy.transform.position.x,
                posY = enemy.transform.position.y,
                velX = rigidbody.linearVelocity.x,
                velY = rigidbody.linearVelocity.y,
                hp = hm.hp

            };

            return output;
        }

        public static HeroData? getHeroData()
        {
            var hero = HeroController.instance;
            var rigidbody = hero.GetComponent<Rigidbody2D>();

            if (rigidbody == null) return null;
            
            HeroData output = new HeroData
            {

                posX = hero.transform.position.x,
                posY = hero.transform.position.y,
                velX = rigidbody.linearVelocity.x,
                velY = rigidbody.linearVelocity.y,
                hp = hero.playerData.health,
                silk = hero.playerData.silk,
                canJump = hero.CanJump()

            };

            return output;
        }


        public static EnemyData? getEnemyData(string enemyName)
        {
            var enemy = GameObject.Find(enemyName);
            if (enemy == null) return null;

            return getEnemyData(enemy);
        }

        public static Vector3 GetEnemyPosition(string enemyName)
        {
            var enemy = GameObject.Find(enemyName);
            if (enemy != null)
            {
                return enemy.transform.position;
            }
            return Vector3.zero;
        }


        public static void GetGameObjectComponents(string gameObjectName)
        {
            var obj = GameObject.Find(gameObjectName);
            if (obj == null) return;
            int numComponentes = obj.GetComponentCount();
            SilksongAImod.Log.LogInfo($"Num components: {numComponentes}");
            for (int i = 0; i < numComponentes; i++)
                SilksongAImod.Log.LogInfo(obj.GetComponentAtIndex(i));
        }

        public static PlayMakerFSM[] GetEnemyFSM(string enemyName)
        {
            var enemy = GameObject.Find(enemyName);
            if (enemy != null)
            {
                var fsms = enemy.GetComponents<PlayMakerFSM>();
                return fsms;
            }

            return null;

        }

        public static int GetEnemyHP(string enemyName)
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


        public static GameObject[] GetAllEnemies() //TODO: enemy tracker [HarmonyPatch(typeof(HealthManager), "Awake")]
        {
            var enemies = GameObject.FindObjectsByType<HealthManager>(FindObjectsSortMode.None);
            GameObject[] enemiesGo = new GameObject[enemies.Length];

            for (int i = 0; i < enemies.Length; i++)
            {
                enemiesGo[i] = enemies[i].gameObject;
            }

            return enemiesGo;
        }

        public static GameObject[] GetAllDamageSources()
        {
            var damageSources = GameObject.FindObjectsByType<DamageHero>(FindObjectsSortMode.None);
            GameObject[] damageSourcesGo = new GameObject[damageSources.Length];

            for (int i = 0; i < damageSources.Length; i++)
            {
                damageSourcesGo[i] = damageSources[i].gameObject;
            }

            return damageSourcesGo;
        }

    }

}
