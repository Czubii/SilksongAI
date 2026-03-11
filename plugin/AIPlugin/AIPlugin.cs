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
using GenericVariableExtension;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using UnityEngine.EventSystems;

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
                PluginConfigManager.LoadConfig(Config);

                Log = Logger;
                _registry = new ServiceRegistry();

                var teleporter = gameObject.AddComponent<TeleportService>();
                var aiService = gameObject.AddComponent<AiService>();
                aiService.Initialize("127.0.0.1", 5000);
                var gameStateController = new GameStateController();
                var sessionEventHandler = new SessionEvents();
                var sessionEnemyManager = new SessionEnemyTracker();

                var session = gameObject.AddComponent<SessionOrchestrator>();
                session.Initialize(sessionEventHandler, gameStateController, teleporter, sessionEnemyManager);

                var frameCapturer = gameObject.AddComponent<FrameCapturer>();
                frameCapturer.Initialize(sessionEventHandler, sessionEnemyManager);
                sessionEventHandler.Subscribe(frameCapturer);

                var recorder = gameObject.AddComponent<BossfightRecorder>();
                recorder.Initialize(sessionEnemyManager);
                recorder.enabled = false;
                sessionEventHandler.Subscribe(recorder);

                var aiController = gameObject.AddComponent<AIBossfightController>();
                aiController.Initialize(aiService);
                aiController.enabled = false;
                sessionEventHandler.Subscribe(aiController);

                var test = gameObject.AddComponent<ArtifactCreationWindow>();

                var coordinator = new AIServerCoordinator(aiService);

                
                _registry.Add(teleporter);
                _registry.Add(aiService);
                _registry.Add(gameStateController);
                _registry.Add(sessionEventHandler);
                _registry.Add(sessionEnemyManager);
                _registry.Add(session);
                _registry.Add(frameCapturer);
                _registry.Add(recorder);
                _registry.Add(aiController);
                _registry.Add(coordinator);
                

                gui = new PluginGUI(Config, _registry);

                Harmony.CreateAndPatchAll(typeof(AIPlugin), null);
                Harmony.CreateAndPatchAll(typeof(EnemyTracker), null);
                Harmony.CreateAndPatchAll(typeof(HeroController_LookForInput_Patch), null);
            }
            catch (Exception e)
            {
                Log.LogError(e);
                return;
            }

            Log.LogInfo("Plugin loaded and initialized");
        }

        public class ArtifactCreationWindow : MonoBehaviour
        {
            Rect windowRect = new Rect(20, 20, 120, 50);

            void OnGUI()
            {
                // Register the window. Notice the 3rd parameter
                windowRect = GUILayout.Window(0, windowRect, DoMyWindow, "My Window");
            }

            // Make the contents of the window
            void DoMyWindow(int windowID)
            {
                // This button will size to fit the window
                if (GUILayout.Button("Hello World"))
                {
                    print("Got a click");
                }
            }
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
