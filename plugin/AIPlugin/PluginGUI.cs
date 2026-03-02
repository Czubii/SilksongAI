using AIPlugin.Networking;
using BepInEx.Configuration;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AIPlugin
{
    public class PluginGUI
    {
        private ConfigEntry<bool> godModeToggle;
        private ConfigEntry<bool> showEnemiesToggle;
        private ConfigEntry<bool> teleportToSelectedBossButton;
        private ConfigEntry<bool> startSessionButton;
        private ConfigEntry<bool> connectToServerButton;
        private ConfigEntry<int> bossSelectionDropdown;

        private int _numSessionFights = 1;
        private static readonly int _GUILabelOffsetY = 20;
        public PluginGUI(ConfigFile config)
        {
            godModeToggle = config.Bind(
                  "Cheats",
                  "Infinite Health",
                  false,

                  new ConfigDescription(
                      "Press to enable Infinite Health",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawGodModeToggle
                      }
                  ));
            showEnemiesToggle = config.Bind(
                  "Debug",
                  "Show Enemies On Scene",
                  false,
                  new ConfigDescription(
                      "Press to Show Enemies On Scene",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawShowEnemiesToggle
                      }
                  ));
            bossSelectionDropdown = config.Bind(
                  "Bosses",
                  "Boss selection",
                  0,
                  new ConfigDescription(
                      "Select boss",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawBossSelectionDropdown

                      }
                  ));
            teleportToSelectedBossButton = config.Bind(
                  "Cheats",
                  "Teleport to selected boss",
                  false,
                  new ConfigDescription(
                      "Press to teleport to selected boss",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawTeleportToSelectedBossButton

                      }
                  ));
            startSessionButton = config.Bind(
                  "Bosses",
                  "Start Bossfight Session",
                  false,
                  new ConfigDescription(
                      "Press to Start Bossfight Session",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawStartSessionButton

                      }
                  ));
            connectToServerButton = config.Bind(
                  "Server",
                  "Connect To AI Server",
                  false,
                  new ConfigDescription(
                      "Press to Connect To AI Server",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawConnectToServerButton
                      }
                  ));
        }
        public class ConfigurationManagerAttributes
        {
            public bool? IsAdvanced = null;
            public Action<ConfigEntryBase> CustomDrawer = null;
        }
        private void DrawGodModeToggle(ConfigEntryBase entry)
        {
            bool prev = AIPlugin.GodModeEnabled;
            AIPlugin.GodModeEnabled = GUILayout.Toggle(AIPlugin.GodModeEnabled, "Infinite Health");

            if (prev == false && AIPlugin.GodModeEnabled == true)
                GameStateController.SetFullHP();
        }
        private void DrawShowEnemiesToggle(ConfigEntryBase entry)
        {
            var config = (ConfigEntry<bool>)entry;

            config.Value = GUILayout.Toggle(config.Value, "Show Enemies");
        }
        private bool _showBossDropdown;
        private Vector2 _bossScroll;
        private void DrawBossSelectionDropdown(ConfigEntryBase entry)
        {
            var config = (ConfigEntry<int>)entry;

            string[] bossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToArray(); 

            if (!_showBossDropdown) { 
                // Button showing current selection
                if (GUILayout.Button(bossNames[config.Value]))
                {
                    _showBossDropdown = !_showBossDropdown;
                }
            }

            if (_showBossDropdown)
            {
                GUILayout.BeginVertical("box");

                _bossScroll = GUILayout.BeginScrollView(
                    _bossScroll,
                    GUIStyle.none,
                    GUILayout.Height(200)   // visible height of dropdown
                );

                for (int i = 0; i < bossNames.Length; i++)
                {
                    if (GUILayout.Button(bossNames[i]))
                    {
                        config.Value = i;
                        _showBossDropdown = false;
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

        }
        private void DrawTeleportToSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossMetaData bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            if (!CanUseTeleportButton())
                GUI.enabled = false;

            if (GUILayout.Button($"Teleport to {bossReference.DisplayName}"))
            {
                bossReference.SetDefeated(true);
                TeleportService.instance.TeleportTo(bossReference, true);
            }

            GUI.enabled = oldGUIenabled;
        }
        private void DrawConnectToServerButton(ConfigEntryBase entry)
        {
            bool oldGUIenabled = GUI.enabled;
            var aiService = AiService.instance;

            if (aiService == null || aiService.IsConnected)
                GUI.enabled = false;

            if (GUILayout.Button($"Connect To AI Server"))
            {
                aiService?.ConnectToServer();
            }

            GUI.enabled = oldGUIenabled;
        }
        private void DrawStartSessionButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossMetaData bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            GUILayout.BeginVertical();

            if (!CanUseTeleportButton() || _numSessionFights == 0)
                GUI.enabled = false;

            if (GUILayout.Button($"Start Bossfight Session"))
            {
                BossFightSession.instance.StartSession(bossReference, _numSessionFights);
            }
            GUI.enabled = true;
            GUILayout.BeginHorizontal();

            GUILayout.Label("Number Of Fights: ");

            string _numRecordingsStr = "";
            if (_numSessionFights != 0)
                _numRecordingsStr = _numSessionFights.ToString();
            _numRecordingsStr = GUILayout.TextField(_numRecordingsStr, 3, GUILayout.Width(35));
            _numRecordingsStr = RemoveNonNumberChar(_numRecordingsStr);

            if (_numRecordingsStr.Length > 0)
                _numSessionFights = int.Parse(_numRecordingsStr, System.Globalization.NumberStyles.Integer);
            else _numSessionFights = 0;

            _numSessionFights = Math.Abs(_numSessionFights);


            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Recording Enabled: ");
            GUILayout.FlexibleSpace();

            var recorder = BossFightRecorder.instance;
            if (recorder == null)
            {
                GUI.enabled = false;
                GUILayout.Toggle(false, "");
                GUI.enabled = true;
            }
            else if (BossFightSession.instance.State != BossFightSession.SessionState.Idle)
            {
                GUI.enabled = false;
                GUILayout.Toggle(recorder.enabled, "");
                GUI.enabled = true;
            }
            else recorder.enabled = GUILayout.Toggle(recorder.enabled, "");


            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();


            GUILayout.Label("AI Enabled: ");
            GUILayout.FlexibleSpace();
           // BossFightRecorder.instance. = GUILayout.Toggle(_sessionAiEnabled, "");


            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUI.enabled = oldGUIenabled;
        }

        private static string RemoveNonNumberChar(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }
        private bool CanUseTeleportButton()
        {
            if(!TeleportService.instance.CanTeleport() || 
                BossFightSession.instance.State != BossFightSession.SessionState.Idle)
                return false;
            
            return true;
        }
        private static readonly GUIStyle labelStyleRight = new GUIStyle
        {
            fontSize = 18,
            normal = {textColor = Color.white},
            alignment = TextAnchor.MiddleRight
            
        };
        private static readonly GUIStyle labelStyleLeft = new GUIStyle
        {
            fontSize = 18,
            normal = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft

        };
        public void OnGUIDrawStateLabel()
        {

            Rect rect0 = new Rect(Screen.width - 200, 10, 180, 9);

            try 
            {
                var Scene = SceneManager.GetActiveScene();
                GUI.Label(rect0, $"Scene: {Scene.name}", labelStyleRight);
                rect0.y += _GUILabelOffsetY;
            }
            catch (Exception e)
            {
                AIPlugin.Log.LogError($"OnGUIDrawStateLabel(): {e}");
            }

            HeroController HC = HeroController.instance;
            if (HC != null)
                GUI.Label(rect0, $"Pos: ({HC.transform.position.x,5:0.0}, {HC.transform.position.y,5:0.0})", labelStyleRight);
            else
                GUI.Label(rect0, "Pos: No Hero On Scene", labelStyleRight);

            rect0.y += _GUILabelOffsetY;

            var bf = BossFightSession.instance;
            if (bf.State != BossFightSession.SessionState.Idle)
            {
                string BossName = bf.TargetBoss.DisplayName;
                int RecordedFights = bf.TotalFights - bf.RemainingFights;
                int TotalFights = bf.TotalFights;
                switch (bf.State)
                {
                    case BossFightSession.SessionState.StartingNewFight:
                        GUI.Label(rect0, $"Starting fight: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                    case BossFightSession.SessionState.AwaitingBoss:
                        GUI.Label(rect0, $"Awating boss: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                    case BossFightSession.SessionState.Fighting:
                        GUI.Label(rect0, $"Fighting: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                    case BossFightSession.SessionState.Stopping:
                        GUI.Label(rect0, $"Stopping fight: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                };
                
            }
            else
            {
                GUI.Label(rect0, $"Fighting Session: Not Fighting", labelStyleRight);
            }


            int numEnemies = EnemyTracker.GetCount();
            int labelEnemiesStartY = Screen.height - ((numEnemies + 2) * _GUILabelOffsetY);
            Rect labelEnemiesRect0 = new Rect(20, labelEnemiesStartY, 180, 9);

            if (showEnemiesToggle.Value)
            {
                if (numEnemies > 0)
                    GUI.Label(labelEnemiesRect0, $"Enemies: ", labelStyleLeft);
                else
                    GUI.Label(labelEnemiesRect0, $"Enemies: None", labelStyleLeft);

                foreach(var enemy in EnemyTracker.GetAll())
                {
                    labelEnemiesRect0.y += _GUILabelOffsetY;

                    GUI.Label(labelEnemiesRect0, $"{enemy.Name}", labelStyleLeft);
                    
                }
            }
        }

    }
}
