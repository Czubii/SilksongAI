using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Collections;

namespace SilksongAI
{
    public static class TeleportUtils
    {
        public static bool TeleportInProgress { get; private set; }
        private static MonoBehaviour Runner;

        private static void EnsureRunner()
        {
            if (Runner != null) return;

            var go = new GameObject("TeleportUtilsRunner");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Runner = go.AddComponent<CoroutineRunner>();
        }

        private class CoroutineRunner : MonoBehaviour { }

        public static void TeleportTo(BossReference boss)
        {
            TeleportTo(boss.ArenaMapName, boss.ArenaPosition);
        }
        public static void TeleportTo(string sceneName, Vector3 pos)
        {
            if (TeleportInProgress)
            {
                SilksongAImod.Log.LogWarning("Cannot teleport, the old one is still in progress");
                return;
            }
            if(!CanPerformTeleportOperations())
            {
                SilksongAImod.Log.LogWarning("Cannot teleport.");
                return;
            }

            TeleportInProgress = true;

            GameManager.instance.BeginSceneTransition(new GameManager.SceneLoadInfo
            {
                SceneName = sceneName,
                EntryGateName = "left1",
                HeroLeaveDirection = GlobalEnums.GatePosition.unknown,
                EntryDelay = 0f,
                Visualization = GameManager.SceneLoadVisualizations.Default,
                AlwaysUnloadUnusedAssets = true
            });
            EnsureRunner();
            Runner.StartCoroutine(TeleportHeroWhenPossible(pos));
        }

        private static IEnumerator TeleportHeroWhenPossible(Vector3 pos)
        {
            yield return new WaitWhile(() =>
            {

                var GM = GameManager.instance;
                var HC = HeroController.instance;

                if (GM == null || HC == null) return true;

                return !HC.isHeroInPosition || HC.cState.transitioning || GM.IsInSceneTransition;

            });

            yield return new WaitUntil(() =>
            {
                var HC = HeroController.instance;
                return HC != null && HC.CanInput();
            });

            TeleportHero(pos);
        }

        private static void TeleportHero(Vector3 pos)
        {            
            if (HeroController.instance == null)
            {
                SilksongAImod.Log.LogWarning("Cannot teleport, no HeroController.instance on scene");
                TeleportInProgress = false;
                return;
            }

            HeroController.instance.transform.position = pos;

            var HeroRigidbody2D = HeroController.instance.GetComponent<Rigidbody2D>();
            if (HeroRigidbody2D != null)
            {
                HeroRigidbody2D.linearVelocity = Vector2.zero;
            }

            if (HeroController.instance.cState != null)
            {
                HeroController.instance.cState.recoiling = false;
                HeroController.instance.cState.transitioning = false;
            }

            TeleportInProgress = false;
        }


        public static bool CanPerformTeleportOperations()
        {

                if (PlayerData.instance != null && PlayerData.instance.health <= 0)
                    return false;
                
                if (PlayerData.instance != null && PlayerData.instance.atBench)
                    return false;
                
                if (PlayerData.instance != null && !PlayerData.instance.bindCutscenePlayed)
                    return false;
                
                if (GameManager.instance != null && GameManager.instance.RespawningHero)
                    return false;

                return true;
        }
    }

    public static class GetDataUtils
    {
        public static TrainingEnemyData? getEnemyData(GameObject enemy)
        {

            var hm = enemy.GetComponent<HealthManager>();
            if (hm == null) return null;

            var fsm = enemy.GetComponent<PlayMakerFSM>();
            if (fsm == null) return null;

            var rigidbody = enemy.GetComponent<Rigidbody2D>();
            if (rigidbody == null) return null;

            TrainingEnemyData output = new TrainingEnemyData
            {

                posX = enemy.transform.position.x,
                posY = enemy.transform.position.y,
                velX = rigidbody.linearVelocity.x,
                velY = rigidbody.linearVelocity.y,
                hp = hm.hp

            };

            return output;
        }

        public static TrainingHeroData? getHeroData()
        {
            var hero = HeroController.instance;
            var rigidbody = hero.GetComponent<Rigidbody2D>();

            if (rigidbody == null) return null;
            
            TrainingHeroData output = new TrainingHeroData
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


        public static TrainingEnemyData? getEnemyData(string enemyName)
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
