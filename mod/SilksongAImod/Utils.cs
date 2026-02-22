using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using static UnityEngine.EventSystems.EventTrigger;
using Steamworks;

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

        public static void TeleportTo(BossMetaData boss)
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
                EntrySkip = true,
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

            var hc = HeroController.instance;
            var gm = GameManager.instance;
            
            if (hc == null)
            {
                SilksongAImod.Log.LogError("TeleportHero(): Cannot teleport, no HeroController.instance on scene");
                TeleportInProgress = false;
                return;
            }
            if (gm == null) {
                SilksongAImod.Log.LogError("TeleportHero(): Cannot teleport, no GameManager.instance on scene");
                TeleportInProgress = false;
                return;
            }


            hc.transform.position = pos;
            gm.cameraCtrl.PositionToHeroInstant(true);

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
        public static TrainingEnemyData? GetTrainingEnemyData(GameObject enemy)
        {
            if (enemy == null) return null;

            var hm = enemy.GetComponent<HealthManager>();
            if (hm == null) return null;

            var fsms = enemy.GetComponents<PlayMakerFSM>();
            if (fsms == null) return null;

            var rigidbody = enemy.GetComponent<Rigidbody2D>();
            if (rigidbody == null) return null;

            TrainingEnemyData output = new TrainingEnemyData
            {
                posX = enemy.transform.position.x,
                posY = enemy.transform.position.y,
                facing = (int)enemy.transform.GetScaleX(),
                velX = rigidbody.linearVelocity.x,
                velY = rigidbody.linearVelocity.y,
                hp = hm.hp
            };

            output.playMakers = new PlayMaker[fsms.Length];

            for (int i = 0; i < fsms.Length; i++)
            {
                output.playMakers[i].Name = fsms[i].FsmName;
                output.playMakers[i].StateName = fsms[i].ActiveStateName;
            }

            return output;
        }

        public static TrainingHeroData? GetHeroData()
        {
            var hero = HeroController.instance;
            var rigidbody = hero.GetComponent<Rigidbody2D>();

            if (rigidbody == null) return null;

            TrainingHeroData output = new TrainingHeroData
            {

                posX = hero.transform.position.x,
                posY = hero.transform.position.y,
                facing = (int)hero.transform.GetScaleX(),
                velX = rigidbody.linearVelocity.x,
                velY = rigidbody.linearVelocity.y,
                hp = hero.playerData.health,
                silk = hero.playerData.silk,
                canJump = hero.CanJump(),
                canDoubleJump = hero.CanDoubleJump(),
                canAttack = hero.CanAttack(),
                canSprint = hero.CanSprint(),
                canBind = hero.CanBind(),
                canCast = hero.CanCast(),
                canNailArt = hero.CanNailArt(),
                canTryHarpoon = hero.CanTryHarpoonDash(),
                canInput = hero.CanInput(),
                canBackDash = hero.CanBackDash(),

            };

            return output;
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

    public static class PlayerUtils
    {
        public static void SetFullHP()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                SilksongAImod.Log.LogWarning("PlayerUtils.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillHealthToMax();
        }

        public static void SetFullSilk()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                SilksongAImod.Log.LogWarning("PlayerUtils.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillSilkToMaxSilent();
           
        }

        public static void RemoveCocoon()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                SilksongAImod.Log.LogWarning("PlayerUtils.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.CocoonBroken();
  
        }
    }
}
