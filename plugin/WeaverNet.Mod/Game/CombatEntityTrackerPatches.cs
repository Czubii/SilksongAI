using HarmonyLib;
using System;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game
{
    public static class CombatEntityTrackerPatches
    {
        private static ICombatEntityTracker _tracker;

        public static void Initialize(ICombatEntityTracker tracker)
        {
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthManager), "OnEnable")]
        private static void EnemyEnabled(HealthManager __instance)
        {
            if (_tracker == null)
            {
                PluginLog.Error("CombatEntityTrackerPatches: Tracker not initialized before EnemyEnabled.");
                return;
            }

            _tracker.RegisterEnemy(__instance.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthManager), "OnDisable")]
        private static void EnemyDisabled(HealthManager __instance)
        {
            if (_tracker == null)
            {
                PluginLog.Error("CombatEntityTrackerPatches: Tracker not initialized before EnemyDisabled.");
                return;
            }

            _tracker.UnregisterEnemy(__instance.gameObject);
        }
    }
}
