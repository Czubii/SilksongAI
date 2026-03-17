using AIPlugin.BossfightSession;
using BepInEx.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.CullingGroup;

namespace AIPlugin.PluginGUI
{
    /// <summary>
    /// Responsible for starting session / selecting boss / basic session settings like the number of trials and boss selection
    /// </summary>
    public class SessionControlsWindow: BaseWindow//TODO: ENABLE CURSOUR WHEN WINDOW ACTIVE
    {
        private Vector2 _scroll = new Vector2();
        private CustomGUI.DropdownState _bossDropdownState = new CustomGUI.DropdownState();

        private int _num_fights = 1;
        private bool _recordingEnabled = false;
        private bool _aiEnabled = false;
        private bool _keepTools = false;

        private SessionOrchestrator _sessionOrchestrator;
        private BossfightRecorder _recorder;
        private AIBossfightAgent _aiBossfightAgent;

        public SessionControlsWindow(string name, SessionOrchestrator sessionOrchestrator, BossfightRecorder recorder, AIBossfightAgent aiAgent): 
            base(name, new Rect(100, 100, 200, 300))
        {
            _sessionOrchestrator = sessionOrchestrator;
            _recorder = recorder;
            _aiBossfightAgent = aiAgent;
        }
        public override bool CanEnable() => true;
        public override void DrawContent()
        {
            GUILayout.BeginVertical();
            _scroll = GUILayout.BeginScrollView(_scroll, Styles.ScrollView, Styles.VerticalScrollbar, GUILayout.ExpandHeight(true));
            GUI.skin.verticalScrollbarThumb = Styles.VerticalScrollbarThumb;

            if (_sessionOrchestrator.IsSessionActive())
                GUI.enabled = false;

            GUILayout.Label("Boss Selection:");
            List<string> bossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
            _bossDropdownState = CustomGUI.Dropdown(_bossDropdownState, bossNames);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Number of fights: ");
            GUILayout.FlexibleSpace();
            _num_fights = CustomGUI.IntegerField(_num_fights, new GUILayoutOption[] { GUILayout.Width(50) });
            GUILayout.EndHorizontal();


            _recordingEnabled = CustomGUI.Toggle(_recordingEnabled, "Record: ");
            if (_recorder.enabled != _recordingEnabled) _recorder.enabled = _recordingEnabled;

            _aiEnabled = CustomGUI.Toggle(_aiEnabled, "Enable AI: ");
            if (_aiBossfightAgent.enabled != _aiEnabled) _aiBossfightAgent.enabled = _aiEnabled;

            _keepTools = CustomGUI.Toggle(_keepTools, "Keep Tools: ");

            GUILayout.EndScrollView();
            if (!_sessionOrchestrator.CanStart()) // TODO add check if game is paused as it breaks 
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Start Session", Styles.Button) && _sessionOrchestrator.CanStart())
            {
                var boss = BossReferenceDatabase.All.ToList()[_bossDropdownState.SelectedIdx];
                var context = new SessionContext(boss, _num_fights, 
                    new SessionContext.SessionSettings(false, false));

                _sessionOrchestrator.TryStart(context);
            }

            GUI.enabled = true;

            if (!_sessionOrchestrator.IsSessionActive())
            {
                GUI.enabled = false;
            }
            if (GUILayout.Button("Stop Session", Styles.Button) && _sessionOrchestrator.IsSessionActive())
            {
                _sessionOrchestrator.RequestStop();
            }

            GUILayout.EndVertical();
            GUI.enabled = true;
        }
    }
}
