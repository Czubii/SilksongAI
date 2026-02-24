using BepInEx;
using BepInEx.Configuration;
using HutongGames.PlayMaker.Actions;
using Steamworks;
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

        private ConfigEntry<bool> godModeToggle;
        private ConfigEntry<bool> showEnemiesToggle;
        private ConfigEntry<bool> fightSelectedBossButton;
        private ConfigEntry<bool> teleportToSelectedBossButton;
        private ConfigEntry<bool> recordSelectedBossButton;
        private ConfigEntry<int> bossSelectionDropdown;

        private SilksongAImod plugin;

        public ModGUI(SilksongAImod plugin)
        {
            this.plugin = plugin;

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

            showEnemiesToggle = plugin.Config.Bind(
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

        }

        public class ConfigurationManagerAttributes
        {
            public bool? IsAdvanced = null;
            public Action<ConfigEntryBase> CustomDrawer = null;
        }

        private void DrawGodModeToggle(ConfigEntryBase entry)
        {
            GodModeEnabled = GUILayout.Toggle(GodModeEnabled, "GodMode");
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

        private void DrawFightSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossMetaData bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            if (!CanUseTeleportButton())
                GUI.enabled = false;

            if (GUILayout.Button($"Fight {bossReference.DisplayName}"))
            {
                bossReference.SetDefeated(false);

                TeleportService.instance.TeleportTo(bossReference, true);
            }

            GUI.enabled = oldGUIenabled;
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

        private int _numRecordings = 1;
        private void DrawRecordSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossMetaData bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            GUILayout.BeginHorizontal();

            if (!CanUseTeleportButton() || _numRecordings == 0)
                GUI.enabled = false;

            if (GUILayout.Button($"Record {bossReference.DisplayName} Fight"))
            {
                BossFightSession.instance.StartSession(bossReference, _numRecordings);
               //BossFightRecordingSession.StartSession(bossReference, _numRecordings);
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

        private static readonly int GUILabelOffsetY = 20;
        public void OnGUIDrawStateLabel()
        {

            Rect rect0 = new Rect(Screen.width - 200, 10, 180, 9);

            try 
            {
                var Scene = SceneManager.GetActiveScene();
                GUI.Label(rect0, $"Scene: {Scene.name}", labelStyleRight);
                rect0.y += GUILabelOffsetY;
            }
            catch (Exception e)
            {
                SilksongAImod.Log.LogError($"OnGUIDrawStateLabel(): {e}");
            }

            HeroController HC = HeroController.instance;
            if (HC != null)
                GUI.Label(rect0, $"Pos: ({HC.transform.position.x,5:0.0}, {HC.transform.position.y,5:0.0})", labelStyleRight);
            else
                GUI.Label(rect0, "Pos: No Hero On Scene", labelStyleRight);

            rect0.y += GUILabelOffsetY;

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
            int labelEnemiesStartY = Screen.height - ((numEnemies + 2) * GUILabelOffsetY);
            Rect labelEnemiesRect0 = new Rect(20, labelEnemiesStartY, 180, 9);

            if (showEnemiesToggle.Value)
            {
                if (numEnemies > 0)
                    GUI.Label(labelEnemiesRect0, $"Enemies: ", labelStyleLeft);
                else
                    GUI.Label(labelEnemiesRect0, $"Enemies: None", labelStyleLeft);

                int y = 0;
                foreach(var enemy in EnemyTracker.GetAll())
                {
                    labelEnemiesRect0.y += GUILabelOffsetY;

                    GUI.Label(labelEnemiesRect0, $"{enemy.Name}", labelStyleLeft);
                    
                }
            }
        }

    }
}
