using BepInEx;
using BepInEx.Configuration;
using HutongGames.PlayMaker.Actions;
using System;
using System.Linq;
using TMProOld;
using UnityEngine;
using UnityEngine.SceneManagement;

using static SilksongAImod;

namespace SilksongAI
{
    public class ModGUI
    {

        private ConfigEntry<bool> getPositionButton;
        private ConfigEntry<bool> printEnemiesButton;
        private ConfigEntry<bool> godModeToggle;
        private ConfigEntry<bool> fightSelectedBossButton;
        private ConfigEntry<bool> teleportToSelectedBossButton;
        private ConfigEntry<bool> recordSelectedBossButton;
        private ConfigEntry<int> bossSelectionDropdown;

        private SilksongAImod plugin;

        public ModGUI(SilksongAImod plugin)
        {
            this.plugin = plugin;

            getPositionButton = plugin.Config.Bind(
                    "Debug",
                    "Print Hornet Position",
                    false,
                    new ConfigDescription(
                        "Press to Print Hornet Position",
                        null,
                        new ConfigurationManagerAttributes
                        {
                            IsAdvanced = false,
                            CustomDrawer = DrawPrintPositionButton
                        }
                    ));

            godModeToggle = plugin.Config.Bind(
                  "General",
                  "Enable god mode",
                  false,
                  new ConfigDescription(
                      "Press to enable god mode",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawGodModeToggle
                      }
                  ));



            bossSelectionDropdown = plugin.Config.Bind(
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
                


            fightSelectedBossButton = plugin.Config.Bind(
                  "Bosses",
                  "Fight selected boss",
                  false,
                  new ConfigDescription(
                      "Press to fight selected boss",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawFightSelectedBossButton

                      }
                  ));

            teleportToSelectedBossButton = plugin.Config.Bind(
                  "Bosses",
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

            recordSelectedBossButton = plugin.Config.Bind(
                  "Bosses",
                  "Record the selected boss fight",
                  false,
                  new ConfigDescription(
                      "Press to record the selected boss fight",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawRecordSelectedBossButton

                      }
                  ));


            printEnemiesButton = plugin.Config.Bind(
                  "Debug",
                  "Log print enemies",
                  false,
                  new ConfigDescription(
                      "Press to Log print enemies",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawPrintEnemiesButton

                      }
                  ));
        }

        public class ConfigurationManagerAttributes
        {
            public bool? IsAdvanced = null;
            public Action<ConfigEntryBase> CustomDrawer = null;
        }


        private void DrawPrintPositionButton(ConfigEntryBase entry)// TODO remove or make safe
        {
            if (GUILayout.Button("Print Hornet Position"))
            {
                var hero = HeroController.instance;
                string scene_name = GameManager.instance.sceneName;
                if (hero != null)
                {
                    Vector3 pos = hero.transform.position;
                    Log.LogInfo($"Player position: {pos}");
                    Log.LogInfo($"Scene: {scene_name}");
                }

            }
        }

        private void DrawGodModeToggle(ConfigEntryBase entry)
        {
            GodModeEnabled = GUILayout.Toggle(GodModeEnabled, "GodMode");
        }

        private void DrawPrintEnemiesButton(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Log Print Enemies"))
            {
                foreach (var hm in UnityEngine.Object.FindObjectsByType<HealthManager>(FindObjectsSortMode.None))
                {
                    Log.LogInfo(hm.gameObject.name);
                }
            }
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

        private void DrawFightSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossReference bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            if (!CanUseTeleportButton())
                GUI.enabled = false;

            if (GUILayout.Button($"Fight {bossReference.DisplayName}"))
            {
                bossReference.SetDefeated(false);

                TeleportUtils.TeleportTo(bossReference.ArenaMapName, bossReference.ArenaPosition);
            }

            GUI.enabled = oldGUIenabled;
        }
        private void DrawTeleportToSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossReference bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            if (!CanUseTeleportButton())
                GUI.enabled = false;

            if (GUILayout.Button($"Teleport to {bossReference.DisplayName}"))
            {
                bossReference.SetDefeated(true);
                TeleportUtils.TeleportTo(bossReference.ArenaMapName, bossReference.ArenaPosition);
            }

            GUI.enabled = oldGUIenabled;
        }

        private int _numRecordings = 1;
        private void DrawRecordSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossReference bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            GUILayout.BeginHorizontal();

            if (!CanUseTeleportButton() || _numRecordings == 0)
                GUI.enabled = false;

            if (GUILayout.Button($"Record {bossReference.DisplayName} Fight"))
            {
               BossFightRecordingSession.StartSession(bossReference, _numRecordings);
            }

            GUI.enabled = oldGUIenabled;


            string _numRecordingsStr = "";

            if (_numRecordings != 0)
                _numRecordingsStr = _numRecordings.ToString();

            _numRecordingsStr = GUILayout.TextField(_numRecordingsStr, 3, GUILayout.Width(35));

            _numRecordingsStr = RemoveNonNumberChar(_numRecordingsStr);

            if (_numRecordingsStr.Length > 0)
                _numRecordings = int.Parse(_numRecordingsStr, System.Globalization.NumberStyles.Integer);
            else
                _numRecordings = 0;

            _numRecordings = Math.Abs(_numRecordings);


            GUILayout.EndHorizontal();

            
        }

        private static string RemoveNonNumberChar(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
        }

        private bool CanUseTeleportButton()
        {
            if(!TeleportUtils.CanPerformTeleportOperations() || BossFightRecordingSession.SessionActive)
                return false;
            
            return true;
        }

        
        private static readonly GUIStyle labelStyle = new GUIStyle
        {
            fontSize = 18,
            normal = {textColor = Color.white},
            alignment = TextAnchor.MiddleRight
            
        };
        public void OnGUIDrawStateLabel()
        {

            Rect labelSceneRect = new Rect(Screen.width - 200, 10, 
                                           180, 9);

            Rect labelPosSceneRect = new Rect(Screen.width - 200, 30, 
                                              180, 9);

            Rect labelrecordingInfoRect = new Rect(Screen.width - 200, 50,
                                  180, 9);


            try // TODO look at this closer (can we check wether the instance exists?
            {
                var Scene = SceneManager.GetActiveScene();
                GUI.Label(labelSceneRect, $"Scene: {Scene.name}", labelStyle);
            }
            catch (Exception e)
            {
                SilksongAImod.Log.LogError(e);
                GUI.Label(labelSceneRect, $"Scene: {e.Message}", labelStyle);
            }

            HeroController HC = HeroController.instance;
            if (HC != null)
                GUI.Label(labelPosSceneRect, $"Pos: ({HC.transform.position.x,5:0.0}, {HC.transform.position.y,5:0.0})", labelStyle);
            else
                GUI.Label(labelPosSceneRect, "Pos: No Hero On Scene", labelStyle);

            if (BossFightRecordingSession.SessionActive)
            {
                string BossName = BossFightRecordingSession.TargetBoss.DisplayName;
                int RecordedFights = BossFightRecordingSession.Fights - BossFightRecordingSession.RemainingFights;
                int TotalFights = BossFightRecordingSession.Fights;
                GUI.Label(labelrecordingInfoRect, $"Recording: {BossName} {RecordedFights}/{TotalFights}", labelStyle);
            }
            else
            {
                GUI.Label(labelrecordingInfoRect, $"Recording: Not Recordnig", labelStyle);
            }
        }

    }
}
