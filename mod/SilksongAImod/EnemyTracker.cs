using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SilksongAI
{
    public class Enemy
    {
        public string Name; 
        public GameObject GameObject;

    }
    public static class EnemyTracker
    {
        private static Dictionary<GameObject, Enemy> _enemies = 
            new Dictionary<GameObject, Enemy>();

        public static int GetCount()
        {
            return _enemies.Count;
        }
        public static IEnumerable<Enemy> GetAll()
        {
            return _enemies.Values;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthManager), "OnEnable")]
        private static void EnemyEnabled(HealthManager __instance)
        {
            _enemies[__instance.gameObject] = new Enemy
            {
                Name = __instance.name,
                GameObject = __instance.gameObject,
            };
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthManager), "OnDisable")]
        private static void EnemyDisabled(HealthManager __instance)
        {
            _enemies.Remove(__instance.gameObject);
        }


    }
}
