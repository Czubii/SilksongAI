using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.IO;
using UnityEngine;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Mod.Game.Bosses;
using WeaverNet.Mod.PluginGUI;
using WeaverNet.Mod.PluginGUI.Windows;

namespace WeaverNet.Mod
{
    [BepInPlugin(
        "com.weavernet.mod",
        "WeaverNet Mod",
        "0.1.0"
    )]
    public class WeaverNetPlugin : BaseUnityPlugin
    {
        private ConfigEntry<string> _bossMetadataPath;
        private void Awake()
        {
            Logger.LogInfo("WeaverNet Mod loading...");

            PluginRuntime.Initialize();
            var logger = new BepInExPluginLogger(Logger);
            PluginLog.Bind(logger);


            DataDrivenBossDatabase bossDatabase = new DataDrivenBossDatabase(Path.Combine(Paths.PluginPath, "WeaverNet", "Data", "Bosses"));
            bossDatabase.Load();

            Logger.LogInfo("Configs Loaded Successfully");



            var windowManager = gameObject.AddComponent<PluginWindowManager>();
            windowManager.Initialize(Config);

            var utilitiesWindow = new UtilitiesWindow("Utilities");

            windowManager.Register(utilitiesWindow, true);

            Logger.LogInfo("Patching started...");
            Harmony.CreateAndPatchAll(typeof(CursorPatcher), null);
            Logger.LogInfo("Everything loaded successfully");

        }
        private void LoadConfigs()
        {
            _bossMetadataPath = Config.Bind(
                    "Paths",
                    "Boss Metadata Directory Path",
                    Path.Combine(Paths.PluginPath, "BossMetadata"),
                    "Specify the absolute path to the directory."
                );
        }
        private void OnDestroy()
        {
            Logger.LogInfo("WeaverNet Mod shutdown.");
        }
    }
}