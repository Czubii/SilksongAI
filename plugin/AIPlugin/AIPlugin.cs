using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;
using Steamworks;
using AIPlugin.Networking;
using System.Threading.Tasks;
using AIPlugin.BossfightSession;
using System.Collections.Generic;
using AIPlugin.Utilities;
using GenericVariableExtension;
using System.Reflection;
using HutongGames.PlayMaker.Actions;
using UnityEngine.EventSystems;
using AIPlugin.PluginGUI;
using AIPlugin.PluginGUI.Windows;

namespace AIPlugin
{
    [BepInPlugin("com.czubii.AIPlugin", "AI Plugin", "1.0.0")]
    public class AIPlugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; private set; }
        public static string SteamUserName = "Unknown";
        private void Awake()
        {
            try
            {
                RewardCalculator.Bind(Config);

                Log = Logger;

                gameObject.AddComponent<MainThreadDispatcher>();
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

                var aiController = gameObject.AddComponent<AIBossfightAgent>();
                aiController.Initialize(aiService);
                aiController.enabled = false;
                sessionEventHandler.Subscribe(aiController);

                var artifactSelection = new ClientState();

                var sessionDispatcher = new SessionDispatcher(artifactSelection, session, recorder, aiController, aiService);

                var sessionControlsWindow = 
                    new SessionControlsWindow("Session Controls", artifactSelection, sessionDispatcher);
                var serverControlsWindow = 
                    new ServerControlsWindow("Server Controls", aiService);
                var artifactCreatorWindow = 
                    new ArtifactCreatorWindow("Artfiact Creator", aiService);
                var artifactTrainingWindow = 
                    new BehavioralCloningWindow("Behavioral Cloning", artifactSelection, aiService);
                var artifactManagerWIndow =
                    new ArtifactManager("Artifact settings", artifactSelection, aiService);

                var gameStateScreenLabel = gameObject.AddComponent<GameStateScreenLabel>();
                gameStateScreenLabel.Initialize(Config, session, aiController, recorder);

                var enemyTrackerScreenLabel = gameObject.AddComponent<EnemyTrackerScreenLabel>();
                enemyTrackerScreenLabel.Initialize(Config);

                var windowManager = gameObject.AddComponent<PluginWindowManager>();
                windowManager.Initialize(Config);
                windowManager.Register(serverControlsWindow, true);
                windowManager.Register(sessionControlsWindow, true);
                windowManager.Register(artifactManagerWIndow, true);
                windowManager.Register(artifactCreatorWindow, true);
                windowManager.Register(artifactTrainingWindow, true);
                
                windowManager.Register(enemyTrackerScreenLabel);
                windowManager.Register(gameStateScreenLabel);

                Harmony.CreateAndPatchAll(typeof(AIPlugin), null);
                Harmony.CreateAndPatchAll(typeof(EnemyTracker), null);
                Harmony.CreateAndPatchAll(typeof(BossfightSession.Agents.AIInputPatcher), null);
                Harmony.CreateAndPatchAll(typeof(CursorManager), null);
            }
            catch (Exception e)
            {
                Log.LogError($"ERROR WHEN INITIALIZING PLUGIN: {e}");
                return;
            }
            Log.LogInfo("Plugin loaded and initialized");
        }
        void Update()
        {
            ThreadSafeLogService.Flush();
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
    }
}
