using AIPlugin.BossfightSession;
using AIPlugin.BossfightSession.Agents;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AIPlugin.PluginGUI
{
    public class GameStateScreenLabel : BaseScreenLabel 
    {

        private static readonly GUIStyle _labelStyle = new GUIStyle
        {
            fontSize = 18,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleRight
        };

        private ConfigFile _configFile;
        private ConfigEntry<bool> _enabledInConfig;

        private SessionOrchestrator _sessionOrchestrator;
        private BossfightRecorder _recorder;
        private AgentManager _agentManager;

        public void Initialize(
            ConfigFile config, 
            SessionOrchestrator sessionOrchestrator,
            AgentManager agentManager,
            BossfightRecorder recorder)
        {
            _agentManager = agentManager;   
            _sessionOrchestrator = sessionOrchestrator;
            _recorder = recorder;
            _enabledInConfig = config.Bind("Labels", "Show Game State", true);
        }
        public override bool EnabledInConfig() => _enabledInConfig.Value;
        private void OnGUI()
        {
            Rect rect0 = new Rect(Screen.width - 200, 10, 180, 9);

            try
            {
                var Scene = SceneManager.GetActiveScene();
                GUI.Label(rect0, $"Scene: {Scene.name}", _labelStyle);
                rect0.y += _lineOffsetY;
            }
            catch (Exception e)
            {
                AIPlugin.Log.LogError($"OnGUIDrawStateLabel(): {e}");
            }

            HeroController HC = HeroController.instance;
            if (HC != null)
                GUI.Label(rect0, $"Pos: ({HC.transform.position.x,5:0.0}, {HC.transform.position.y,5:0.0})", _labelStyle);
            else
                GUI.Label(rect0, "Pos: No Hero On Scene", _labelStyle);

            rect0.y += _lineOffsetY;

            if (_sessionOrchestrator.State != SessionOrchestrator.SessionState.Idle)
            {
                GUI.Label(rect0, $"Recording: {(_recorder.enabled ? "enabled" : "disabled")}", _labelStyle);
                rect0.y += _lineOffsetY;

                if (_agentManager != null)
                {
                    GUI.Label(rect0, $"Average inference delay: {_agentManager.SelectedAgent.AverageServerDelay(): .0f}ms", _labelStyle);
                    rect0.y += _lineOffsetY;
                }

                string BossName = _sessionOrchestrator.GetTarget().DisplayName;
                int RecordedFights = _sessionOrchestrator.GetCurrentFightIdx();
                int TotalFights = _sessionOrchestrator.GetTotalFightCount();
                switch (_sessionOrchestrator.State)
                {
                    case SessionOrchestrator.SessionState.StartingNewFight:
                        GUI.Label(rect0, $"Starting fight: {BossName} {RecordedFights}/{TotalFights}", _labelStyle);
                        break;
                    case SessionOrchestrator.SessionState.AwaitingBoss:
                        GUI.Label(rect0, $"Awating boss: {BossName} {RecordedFights}/{TotalFights}", _labelStyle);
                        break;
                    case SessionOrchestrator.SessionState.Fighting:
                        GUI.Label(rect0, $"Fighting: {BossName} {RecordedFights}/{TotalFights}", _labelStyle);
                        break;
                    case SessionOrchestrator.SessionState.FinalizingSession:
                        GUI.Label(rect0, $"Stopping fight: {BossName} {RecordedFights}/{TotalFights}", _labelStyle);
                        break;
                };

            }
            else
            {
                GUI.Label(rect0, $"Fighting Session: Inactive", _labelStyle);
            }
        }
    }
}
