using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using Steamworks;
using HarmonyLib.Tools;
using AIPlugin.Networking;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin
{
    [BepInPlugin("com.czubii.AIPlugin", "AI Plugin", "1.0.0")]
    public class AIPlugin : BaseUnityPlugin
    {
        private PluginGUI gui;
        private Task test;
        public static ManualLogSource Log { get; private set; }
        public static string SteamUserName = "Unknown";
        public static bool GodModeEnabled { get; set; } = false;
        private void Awake()
        {
            try
            {
                Log = Logger;

                TeleportService.EnsureExists();
                BossFightSession.EnsureExists();
                BossfightRecorder.EnsureExists();
                BossfightRecorder.instance.enabled = false;
                AiService.EnsureExists();

                gui = new PluginGUI(Config);

                HarmonyFileLog.Enabled = true;
                Harmony.CreateAndPatchAll(typeof(AIPlugin), null);
                Harmony.CreateAndPatchAll(typeof(EnemyTracker), null);
            }
            catch (Exception e)
            {
                Log.LogError(e);
                return;
            }

            Log.LogInfo("Plugin loaded and initialized");
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(SteamAPI), "Init")]
        private static void SteamAPIInitPostFix()
        {
            try
            {
                if (SteamAPI.IsSteamRunning())
                {
                    SteamUserName = SteamFriends.GetPersonaName();
                    AIPlugin.Log.LogMessage($"Player name used for recording info: {SteamUserName}");
                }
            }
            catch (Exception e)
            {
                AIPlugin.Log.LogWarning($"Steam name failed: {e}");
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                //GameStateLogger.LogGameStateToFiles();
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                //BossFightRecordingSession.ForceStopRecordingSession();
                BossFightSession.instance.StopSession();
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                test = Test();
            }
        }

        private async Task Test()
        {
            var models = await AiService.instance.gateway.ListModelsAsync();
            foreach (var model in models)
            {
                Log.LogInfo($"{model}");
            }
        }
        private void OnGUI()
        {
            gui.OnGUIDrawStateLabel();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), "TakeHealth")]
        private static void TakeHealthPostfix(PlayerData __instance, int amount, bool hasBlueHealth, bool allowFracturedMaskBreak)
        {
            if (GodModeEnabled)
            {
                __instance.health = __instance.maxHealth;
            }
        }
    }
}
