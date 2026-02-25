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
   

    public static class GetDataUtils
    {
        

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
