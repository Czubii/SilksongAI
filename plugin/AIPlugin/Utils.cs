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
using TeamCherry.SharedUtils;
using static DamageReference;
using static GameManager;
using InControl;

namespace AIPlugin
{
    //TODO get rid of all of this by moving to some better places
    public class CustomRespawnPoint
    {
        private RespawnMarker _marker;
        private string _scene;
        private GameObject _go;
        private bool _disposed;
        public CustomRespawnPoint(string name, string scene, Vector3 position)
        {
            _scene = scene;
            _go = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(_go);
            _marker = _go.AddComponent<RespawnMarker>();

            // Initialize minimal required fields
            _marker.customWakeUp = false;
            _marker.customFadeDuration = new OverrideFloat
            {
                Value = 0f,
                IsEnabled = false
            };

            _marker.transform.position = position;
            SceneTeleportMap.AddRespawnPoint(scene, name);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            ResetTemporary();

            if (_go != null)
            {
                UnityEngine.Object.Destroy(_go);
                _go = null;
            }

            _marker = null;

        }
        public void UseAsTemporary(int type = 0)
        {
            var pd = PlayerData.instance;
            if (pd == null)
            {
                AIPlugin.Log.LogError("CustomRespawnPoint.UseAsTemporary(): no PlayerData.instance");
                return;
            }

            pd.tempRespawnMarker = _marker.name;
            pd.tempRespawnScene = _scene;
            pd.tempRespawnType = type;

        }
        public static void ResetTemporary()
        {
            var pd = PlayerData.instance;
            if (pd == null)
            {
                AIPlugin.Log.LogError("CustomRespawnPoint.ResetTemporary(): no PlayerData.instance");
                return;
            }

            pd.ResetTempRespawn();

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
            AIPlugin.Log.LogInfo($"Num components: {numComponentes}");
            for (int i = 0; i < numComponentes; i++)
                AIPlugin.Log.LogInfo(obj.GetComponentAtIndex(i));
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
                AIPlugin.Log.LogWarning("PlayerUtils.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillHealthToMax();
        }

        public static void SetFullSilk()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("PlayerUtils.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.RefillSilkToMaxSilent();
           
        }

        public static void RemoveCocoon()
        {
            HeroController heroController = HeroController.instance;
            if (heroController == null)
            {
                AIPlugin.Log.LogWarning("PlayerUtils.SetFullHP(): no HeroController.instance on scene");
                return;
            }
            heroController.CocoonBroken();
  
        }
    }
}
