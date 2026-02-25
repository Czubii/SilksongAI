using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AIPlugin
{
    public class EnemyInstance
    {
        public string Name; 
        public GameObject GameObject;
        public HealthManager HealthManager;
        public Rigidbody2D Rigidbody2D;
        public PlayMakerFSM[] PlayMakers;
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
                HealthManager = __instance.gameObject.GetComponent<HealthManager>(),
                Rigidbody2D = __instance.gameObject.GetComponent<Rigidbody2D>(),
                PlayMakers = __instance.gameObject.GetComponents<PlayMakerFSM>(),
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
