using AIPlugin.BossfightSession;
using AIPlugin.Infrastructure;
using AIPlugin.Networking;
using AIPlugin.Utilities;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
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

        private TeleportService _teleportService;
        private AiService _aiService;
        private SessionOrchestrator _bossfightSession;
        private BossfightRecorder _bossfightRecorder;
        private GameStateController _gameStateController;
        private AIServerCoordinator _serverCoordinator;
        private AIBossfightController _aiBossfightController;

        private CustomGUILayouts.DropdownState _bossDropdownState = new CustomGUILayouts.DropdownState();
        private CustomGUILayouts.DropdownState _modelDropdownState = new CustomGUILayouts.DropdownState();

        public PluginGUI(ConfigFile config, ServiceRegistry registry)
        {
            _teleportService = registry.Get<TeleportService>();
            _aiService = registry.Get<AiService>();
            _bossfightSession = registry.Get<SessionOrchestrator>();
            _bossfightRecorder = registry.Get<BossfightRecorder>();
            _gameStateController = registry.Get<GameStateController>();
            _serverCoordinator = registry.Get<AIServerCoordinator>();
            _aiBossfightController = registry.Get<AIBossfightController>();

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
                          CustomDrawer = DrawSessionSettings

                      }
                  ));
            connectToServerButton = config.Bind(
                  "AI settings",
                  "Connect To AI Server",
                  false,
                  new ConfigDescription(
                      "Press to Connect To AI Server",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawServerSettings
                      }
                  ));
        }

        private void DrawGodModeToggle(ConfigEntryBase entry)
        {
            bool prev = AIPlugin.GodModeEnabled;
            AIPlugin.GodModeEnabled = GUILayout.Toggle(AIPlugin.GodModeEnabled, "Infinite Health");

            if (prev == false && AIPlugin.GodModeEnabled == true)
                _gameStateController.SetFullHP();
        }
        private void DrawShowEnemiesToggle(ConfigEntryBase entry)
        {
            var config = (ConfigEntry<bool>)entry;

            config.Value = GUILayout.Toggle(config.Value, "Show Enemies");
        }

        private void DrawBossSelectionDropdown(ConfigEntryBase entry)
        {
            var config = (ConfigEntry<int>)entry;
            _bossDropdownState.SelectedIdx = config.Value;
            List<string> bossNames = BossReferenceDatabase.All.Select(s => s.DisplayName).ToList();

            bool oldGUIenabled = GUI.enabled;
            if (!CanUseTeleportButton()) //TODO make more of those functions and make it better lol
                GUI.enabled = false;

            _bossDropdownState = CustomGUILayouts.Dropdown(_bossDropdownState, bossNames);
            config.Value = _bossDropdownState.SelectedIdx;

            GUI.enabled = oldGUIenabled;

        }
        private void DrawTeleportToSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossMetadata bossReference = BossReferenceDatabase.All[bossIdx];

            bool oldGUIenabled = GUI.enabled;

            if (!CanUseTeleportButton())
                GUI.enabled = false;

            if (GUILayout.Button($"Teleport to {bossReference.DisplayName}"))
            {
                _teleportService.TeleportTo(bossReference, true);
            }

            GUI.enabled = oldGUIenabled;
        }

        private void DrawServerSettings(ConfigEntryBase entry)
        {

            bool oldGUIenabled = GUI.enabled;

            if (_aiService.IsConnected)
                GUI.enabled = false;

            GUILayout.BeginVertical();

            if (GUILayout.Button($"Connect To AI Server"))
            {
                _aiService.ConnectToServer();
            }

            if (_aiService.IsConnected)
            {
                GUILayout.BeginHorizontal();
                try
                {
                    GUILayout.Label("Model Selection: ", GUILayout.Width(100));

                    GUI.enabled = true;

                    string bossName = BossReferenceDatabase.All[bossSelectionDropdown.Value].InternalName;
                    _serverCoordinator.EnsureValidSelection(bossName);
                    var modelNames = _serverCoordinator.GetAvailableModels(bossName);
                    int modelIdx = modelNames.IndexOf(_serverCoordinator.GetSelectedModelName());
                    modelIdx = Mathf.Clamp(modelIdx, 0, modelNames.Count - 1);

                    _modelDropdownState.SelectedIdx = modelIdx;

                    _modelDropdownState = CustomGUILayouts.Dropdown(_modelDropdownState, modelNames);

                    if (_modelDropdownState.SelectedIdx != modelIdx)
                        _serverCoordinator.SelectModel(bossName, modelNames[_modelDropdownState.SelectedIdx]);
                }
                catch (Exception ex)
                {
                    AIPlugin.Log.LogError(ex);
                }
                finally
                {
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            GUI.enabled = oldGUIenabled;
        }
        private void DrawSessionSettings(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossMetadata bossReference = BossReferenceDatabase.All[bossIdx];

            bool oldGUIenabled = GUI.enabled;

            GUILayout.BeginVertical();

            if (!CanUseTeleportButton() || _numSessionFights == 0)
                GUI.enabled = false;

            if (GUILayout.Button($"Start Bossfight Session"))
            {
                SessionContext ctx = new SessionContext(bossReference, _numSessionFights);
                _bossfightSession.TryStart(ctx);
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

            if (_bossfightRecorder == null)
            {
                GUI.enabled = false;
                GUILayout.Toggle(false, "");
                GUI.enabled = true;
            }
            else if (_bossfightSession.State != SessionOrchestrator.SessionState.Idle)
            {
                GUI.enabled = false;
                GUILayout.Toggle(_bossfightRecorder.enabled, "");
                GUI.enabled = true;
            }
            else _bossfightRecorder.enabled = GUILayout.Toggle(_bossfightRecorder.enabled, "");


            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("AI Enabled: ");
            GUILayout.FlexibleSpace();

            if (_aiBossfightController == null)
            {
                GUI.enabled = false;
                GUILayout.Toggle(false, "");
                GUI.enabled = true;
            }
            else if (_bossfightSession.State != SessionOrchestrator.SessionState.Idle || !_aiService.IsConnected)
            {
                GUI.enabled = false;
                GUILayout.Toggle(_aiBossfightController.enabled, "");
                GUI.enabled = true;
            }
            else _aiBossfightController.enabled = GUILayout.Toggle(_aiBossfightController.enabled, "");


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
            if(!_teleportService.CanTeleport() ||
                _bossfightSession.State != SessionOrchestrator.SessionState.Idle)
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

            if (_bossfightSession.State != SessionOrchestrator.SessionState.Idle)
            {
                string BossName = _bossfightSession.GetTarget().DisplayName;
                int RecordedFights = _bossfightSession.GetCurrentFightIdx();
                int TotalFights = _bossfightSession.GetTotalFightCount();
                switch (_bossfightSession.State)
                {
                    case SessionOrchestrator.SessionState.StartingNewFight:
                        GUI.Label(rect0, $"Starting fight: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                    case SessionOrchestrator.SessionState.AwaitingBoss:
                        GUI.Label(rect0, $"Awating boss: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                    case SessionOrchestrator.SessionState.Fighting:
                        GUI.Label(rect0, $"Fighting: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                    case SessionOrchestrator.SessionState.FinalizingSession:
                        GUI.Label(rect0, $"Stopping fight: {BossName} {RecordedFights}/{TotalFights}", labelStyleRight);
                        break;
                };
                
            }
            else
            {
                GUI.Label(rect0, $"Fighting Session: Not Fighting", labelStyleRight);
            }


            int numEnemies = EnemyTracker.GetEnemyCount();
            int labelEnemiesStartY = Screen.height - ((numEnemies + 2) * _GUILabelOffsetY);
            Rect labelEnemiesRect0 = new Rect(20, labelEnemiesStartY, 180, 9);

            if (showEnemiesToggle.Value)
            {
                if (numEnemies > 0)
                    GUI.Label(labelEnemiesRect0, $"Enemies: ", labelStyleLeft);
                else
                    GUI.Label(labelEnemiesRect0, $"Enemies: None", labelStyleLeft);

                foreach(var enemy in EnemyTracker.GetAllEnemies())
                {
                    labelEnemiesRect0.y += _GUILabelOffsetY;

                    GUI.Label(labelEnemiesRect0, $"{enemy.Name}", labelStyleLeft);
                    
                }
            }


            int numDmgSources = EnemyTracker.GetDmgSourceCount();
            int labelDmgStartY = Screen.height - ((numDmgSources + numEnemies + 4) * _GUILabelOffsetY);
            Rect labelDmgRect0 = new Rect(20, labelDmgStartY, 180, 9);

            if (showEnemiesToggle.Value)
            {
                if (numDmgSources > 0)
                    GUI.Label(labelDmgRect0, $"Damage Sources: ", labelStyleLeft);
                else
                    GUI.Label(labelDmgRect0, $"Damage Sources: None", labelStyleLeft);

                foreach (var dmg in EnemyTracker.GetAllDmgSources())
                {
                    labelDmgRect0.y += _GUILabelOffsetY;

                    GUI.Label(labelDmgRect0, $"{dmg.Name}", labelStyleLeft);

                }
            }
        }

    }

    public static class CustomGUILayouts
    {
        public class DropdownState
        {
            public int SelectedIdx = 0;
            public bool Expanded = false;
            public Vector2 Scroll;
        }
        public static DropdownState Dropdown(DropdownState state, List<string> options)
        {
            var oldEnabled = GUI.enabled;

            state.SelectedIdx = options.Count <= 0 ? 0 : Mathf.Clamp(state.SelectedIdx, 0, options.Count - 1);

            if ((state.Expanded && !GUI.enabled) || options.Count == 0)
                state.Expanded = false;

            if (!state.Expanded)
            {
                if (options.Count > 0)
                {
                    // Button showing current selection
                    if (GUILayout.Button(options[state.SelectedIdx]))
                    {
                        state.Expanded = !state.Expanded;
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button("No options available");
                }
            }
            else
            {
                GUILayout.BeginVertical("box");

                state.Scroll = GUILayout.BeginScrollView(
                    state.Scroll,
                    GUIStyle.none,
                    GUILayout.Height(200)   // visible height of dropdown
                );

                for (int i = 0; i < options.Count; i++)
                {
                    // Draw a highlight box for the current selection
                    if (i == state.SelectedIdx)
                    {
                        var rect = GUILayoutUtility.GetRect(new GUIContent(options[i]), GUI.skin.button);
                        GUI.Box(rect, "", GUI.skin.box); // TODO MAKE THIS MORE VISIBLE Draw an empty box behind the button
                        if (GUI.Button(rect, options[i]))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(options[i]))
                        {
                            state.SelectedIdx = i;
                            state.Expanded = false;
                        }
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUI.enabled = oldEnabled;

            return state;
        }
    }

}
