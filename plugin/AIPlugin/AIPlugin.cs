using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using Steamworks;
using AIPlugin.Networking;
using System.Threading.Tasks;
using AIPlugin.Infrastructure;
using AIPlugin.BossfightSession;
using System.Collections.Generic;
using AIPlugin.Utilities;

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
        private ServiceRegistry _registry;
        private void Awake()
        {
            try
            {
                Log = Logger;
                _registry = new ServiceRegistry();

                var teleporter = gameObject.AddComponent<TeleportService>();
                var aiService = gameObject.AddComponent<AiService>();
                aiService.Initialize("127.0.0.1", 5000);
                var gameStateController = new GameStateController();
                var sessionEventHandler = new SessionEvents();
                var enemyManager = new SessionEnemyManager();

                var session = gameObject.AddComponent<SessionOrchestrator>();
                session.Initialize(sessionEventHandler, gameStateController, teleporter, enemyManager);

                var recorder = gameObject.AddComponent<BossfightRecorder>();
                recorder.Initialize(enemyManager);
                recorder.enabled = false;
                sessionEventHandler.Subscribe(recorder);

                var aiController = gameObject.AddComponent<AiBossfightController>();
                aiController.Initialize(enemyManager, aiService);
                aiController.enabled = false;
                sessionEventHandler.Subscribe(aiController);

                var coordinator = new AIServerCoordinator(aiService);
                
                _registry.Add(teleporter);//TODO add everything to registry
                _registry.Add(session);
                _registry.Add(recorder);
                _registry.Add(aiService);
                _registry.Add(gameStateController);
                _registry.Add(sessionEventHandler);
                _registry.Add(coordinator);
                _registry.Add(aiController);

                gui = new PluginGUI(Config, _registry);

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
            ThreadSafeLogService.Flush();
            if (Input.GetKeyDown(KeyCode.F11))
            {
                //GameStateLogger.LogGameStateToFiles();
            }
            if (Input.GetKeyDown(KeyCode.F10))
            {
                //BossFightRecordingSession.ForceStopRecordingSession();
                _registry.Get<SessionOrchestrator>().RequestStop();
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
