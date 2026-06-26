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
using AIPlugin.BossfightSession.Agents;

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
                aiService.Initialize();
                var gameStateController = new GameStateController();
                var sessionEventHandler = new SessionEvents();
                var sessionEnemyManager = new SessionEnemyTracker();

                var session = gameObject.AddComponent<SessionOrchestrator>();
                session.Initialize(sessionEventHandler, gameStateController, teleporter, sessionEnemyManager);

                var frameCapturer = gameObject.AddComponent<FrameCapturer>();
                frameCapturer.Initialize(sessionEnemyManager);
                sessionEventHandler.Subscribe(frameCapturer);

                var recorder = gameObject.AddComponent<BossfightRecorder>();
                recorder.Initialize(sessionEnemyManager);
                recorder.enabled = false;
                frameCapturer.Events.Subscribe(recorder);

                var simpleAgent = gameObject.AddComponent<SimpleAgent>();
                simpleAgent.Initialize(aiService);
                simpleAgent.enabled = false;
                frameCapturer.Events.Subscribe(simpleAgent);

                var RLAgent = gameObject.AddComponent<RLAgent>();
                RLAgent.Initialize(aiService);
                RLAgent.enabled = false;
                frameCapturer.Events.Subscribe(RLAgent);

                var agentManager = new AgentManager(simpleAgent, RLAgent);

                var artifactSelection = new ArtifactSelection();

                var sessionDispatcher = new SessionDispatcher(session, recorder, agentManager, aiService);

                var RLManager = gameObject.AddComponent<RLManager>();
                RLManager.Initialize(sessionDispatcher, aiService);

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
                var reinforcementLearningWindow =
                    new ReinforcementLearningWindow("Reinforcement Learning", artifactSelection, aiService);

                var gameStateScreenLabel = gameObject.AddComponent<GameStateScreenLabel>();
                gameStateScreenLabel.Initialize(Config, session, agentManager, recorder);

                var enemyTrackerScreenLabel = gameObject.AddComponent<EnemyTrackerScreenLabel>();
                enemyTrackerScreenLabel.Initialize(Config);

                var windowManager = gameObject.AddComponent<PluginWindowManager>();
                windowManager.Initialize(Config);
                windowManager.Register(serverControlsWindow, true);
                windowManager.Register(sessionControlsWindow, true);
                windowManager.Register(artifactManagerWIndow, true);
                windowManager.Register(artifactCreatorWindow, true);
                windowManager.Register(artifactTrainingWindow, true);
                windowManager.Register(reinforcementLearningWindow, true);
                
                windowManager.Register(enemyTrackerScreenLabel);
                windowManager.Register(gameStateScreenLabel);

                Harmony.CreateAndPatchAll(typeof(AIPlugin), null);
                Harmony.CreateAndPatchAll(typeof(EnemyTracker), null);
                Harmony.CreateAndPatchAll(typeof(AIInputPatcher), null);
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
