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
        private class Form
        {
            public CustomGUI.DropdownState BossDropdownState = new CustomGUI.DropdownState();
            public int NumFights = 1;
            public bool RecordingEnabled = false;
            public bool AiEnabled = false;
            public bool KeepTools = false;

            public Vector2 Scroll = new Vector2();

            public bool IsValid()
            {
                return NumFights > 0;
            }
        }

        private Form form = new Form();

        private SessionOrchestrator _sessionOrchestrator;
        private BossfightRecorder _recorder;
        private AIBossfightAgent _aiBossfightAgent;

        private List<string> _internalBossNames;
        private List<string> _displayBossNames;

        public SessionControlsWindow(string name, SessionOrchestrator sessionOrchestrator, BossfightRecorder recorder, AIBossfightAgent aiAgent): 
            base(name, new Rect(100, 100, 200, 300))
        {
            _sessionOrchestrator = sessionOrchestrator;
            _recorder = recorder;
            _aiBossfightAgent = aiAgent;

            _internalBossNames = BossReferenceDatabase.All.Select(s => s.InternalName).ToList();
            _displayBossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
        }
        public override bool CanEnable() => true;

        private bool CanStart()
        {
            return _sessionOrchestrator.CanStart() && form.IsValid();
        }
        private bool CanStop()
        {
            return _sessionOrchestrator.IsSessionActive();
        }
        public override void DrawContent()
        {
            UI.Form(form)
                .BeginScrollView(x => x.Scroll)
                .Dropdown("Boss:", _displayBossNames, x => x.BossDropdownState)
                .IntegerField("Number of Fights: ", x => x.NumFights)
                .Toggle("Recording Enabled:", x => x.RecordingEnabled)
                .Toggle("Keep Tools:", x => x.KeepTools)
                .EndScrollView()
                .FlexibleSpace()
                .GUIEnabled(CanStart())
                .Button("Start Session", StartSession)
                .GUIEnabled(CanStop())
                .Button("Stop Session", StopSession)
                .End();
        }

        private void StartSession()
        {
            var boss = BossReferenceDatabase.All.ToList()[form.BossDropdownState.SelectedIdx];
            var context = new SessionContext(boss, form.NumFights,
                new SessionContext.SessionSettings(false, false));//TODO add this functionality finally

            _sessionOrchestrator.TryStart(context);
        }
        private void StopSession()
        {
            _sessionOrchestrator.RequestStop();
        }

        //public override void DrawContent()
        //{
        //    GUILayout.BeginVertical();
        //    _scroll = GUILayout.BeginScrollView(_scroll, Styles.ScrollView, Styles.VerticalScrollbar, GUILayout.ExpandHeight(true));
        //    GUI.skin.verticalScrollbarThumb = Styles.VerticalScrollbarThumb;

        //    if (_sessionOrchestrator.IsSessionActive())
        //        GUI.enabled = false;

        //    GUILayout.Label("Boss Selection:");
        //    List<string> bossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();
        //    _bossDropdownState = CustomGUI.Dropdown(_bossDropdownState, bossNames);

        //    GUILayout.BeginHorizontal();
        //    GUILayout.Label("Number of fights: ");
        //    GUILayout.FlexibleSpace();
        //    _num_fights = CustomGUI.IntegerField(_num_fights, new GUILayoutOption[] { GUILayout.Width(50) });
        //    GUILayout.EndHorizontal();


        //    _recordingEnabled = CustomGUI.LabeledToggle(_recordingEnabled, "Record: ");
        //    if (_recorder.enabled != _recordingEnabled) _recorder.enabled = _recordingEnabled;

        //    _aiEnabled = CustomGUI.LabeledToggle(_aiEnabled, "Enable AI: ");
        //    if (_aiBossfightAgent.enabled != _aiEnabled) _aiBossfightAgent.enabled = _aiEnabled;

        //    _keepTools = CustomGUI.LabeledToggle(_keepTools, "Keep Tools: ");

        //    GUILayout.EndScrollView();
        //    if (!_sessionOrchestrator.CanStart()) // TODO add check if game is paused as it breaks 
        //    {
        //        GUI.enabled = false;
        //    }
        //    if (GUILayout.Button("Start Session", Styles.Button) && _sessionOrchestrator.CanStart())
        //    {
        //        var boss = BossReferenceDatabase.All.ToList()[_bossDropdownState.SelectedIdx];
        //        var context = new SessionContext(boss, _num_fights, 
        //            new SessionContext.SessionSettings(false, false));

        //        _sessionOrchestrator.TryStart(context);
        //    }

        //    GUI.enabled = true;

        //    if (!_sessionOrchestrator.IsSessionActive())
        //    {
        //        GUI.enabled = false;
        //    }
        //    if (GUILayout.Button("Stop Session", Styles.Button) && _sessionOrchestrator.IsSessionActive())
        //    {
        //        _sessionOrchestrator.RequestStop();
        //    }

        //    GUILayout.EndVertical();
        //    GUI.enabled = true;
        //}
    }
}
