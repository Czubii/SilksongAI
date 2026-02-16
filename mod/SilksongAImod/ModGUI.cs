using BepInEx;
using BepInEx.Configuration;
using System;
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
        private ConfigEntry<bool> fightMossMotherButton;

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

            fightMossMotherButton = plugin.Config.Bind(
                  "Bosses",
                  "fight moss mother",
                  false,
                  new ConfigDescription(
                      "Press to fight moss mother",
                      null,
                      new ConfigurationManagerAttributes
                      {
                          IsAdvanced = false,
                          CustomDrawer = DrawFightMossMotherButton

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

        private void DrawFightMossMotherButton(ConfigEntryBase entry)
        {
            if (GUILayout.Button("Fight Moss Mother"))
            {
                plugin.RespawnMossMother();

                string scene_name = "Tut_03";
                Vector3 pos = new Vector3(68f, 17.6f, 0);
                plugin.TeleportTo(scene_name, pos);
            }
        }


    }
}
