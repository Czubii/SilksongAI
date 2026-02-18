using BepInEx;
using BepInEx.Configuration;
using System;
using System.Linq;
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


        private void DrawPrintPositionButton(ConfigEntryBase entry)
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
        private void DrawRecordSelectedBossButton(ConfigEntryBase entry)
        {
            int bossIdx = bossSelectionDropdown.Value;
            BossReference bossReference = BossReferenceDatabase.All[bossIdx];


            bool oldGUIenabled = GUI.enabled;

            if (!CanUseTeleportButton())
                GUI.enabled = false;

            if (GUILayout.Button($"Record {bossReference.DisplayName} Fight"))
            {
                BossFightRecordingSession.StartSession(bossReference, 3);
            }

            GUI.enabled = oldGUIenabled;
        }

        private bool CanUseTeleportButton()
        {
            if(!TeleportUtils.CanPerformTeleportOperations() || BossFightRecordingSession.SessionActive)
                return false;
            
            return true;
        }


    }
}
