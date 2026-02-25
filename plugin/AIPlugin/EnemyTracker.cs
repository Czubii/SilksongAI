using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AIPlugin
{
    public class EnemyInstance
    {
        public string Name; 
        public GameObject GameObject;

    }
    public static class EnemyTracker
    {
        private static Dictionary<GameObject, EnemyInstance> _enemies = 
            new Dictionary<GameObject, EnemyInstance>();

        public static int GetCount()
        {
            return _enemies.Count;
        }
        public static IEnumerable<EnemyInstance> GetAll()
        {
            return _enemies.Values;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthManager), "OnEnable")]
        private static void EnemyEnabled(HealthManager __instance)
        {
            _enemies[__instance.gameObject] = new EnemyInstance
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
