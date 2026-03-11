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
    //Nornally enemies should also be here but that is pointless so its just all the game objects with component "DamageHero" and without "HealthManager"
    public class DamageSourceInstance 
    {
        public string Name;
        public GameObject GameObject;
        public Rigidbody2D Rigidbody2D;
        public PlayMakerFSM[] PlayMakers;
    }
    public static class EnemyTracker
    {
        private static Dictionary<GameObject, EnemyInstance> _enemies = 
            new Dictionary<GameObject, EnemyInstance>();

        private static Dictionary<GameObject, DamageSourceInstance> _dmgSources =
            new Dictionary<GameObject, DamageSourceInstance>();

        public static int GetEnemyCount()
        {
            return _enemies.Count;
        }
        public static int GetDmgSourceCount()
        {
            return _dmgSources.Count;
        }
        public static IEnumerable<EnemyInstance> GetAllEnemies()
        {
            return _enemies.Values;
        }
        public static IEnumerable<DamageSourceInstance> GetAllDmgSources()
        {
            return _dmgSources.Values;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthManager), "OnEnable")]
        private static void EnemyEnabled(HealthManager __instance)
        {
            _enemies[__instance.gameObject] = new EnemyInstance
            {
                Name = __instance.gameObject.name,
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DamageHero), "OnEnable")]
        private static void DmgSourceEnabled(DamageHero __instance)
        {
            if(__instance.gameObject.TryGetComponent<HealthManager>(out _))
                return;

            _dmgSources[__instance.gameObject] = new DamageSourceInstance
            {
                Name = __instance.gameObject.name,
                GameObject = __instance.gameObject,
                Rigidbody2D = __instance.gameObject.GetComponent<Rigidbody2D>(),
                PlayMakers = __instance.gameObject.GetComponents<PlayMakerFSM>(),
            };
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DamageHero), "OnDisable")]
        private static void DmgSourceDisabled(DamageHero __instance)
        {
            _dmgSources.Remove(__instance.gameObject);
        }


    }
}
