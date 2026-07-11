using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Mod.Game;
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
        private void Awake()
        {
            try
            {
                Logger.LogInfo("WeaverNet Mod loading...");

                PluginRuntime.Initialize();
                var logger = new BepInExPluginLogger(Logger);
                PluginLog.Bind(logger);


                DataDrivenBossDatabase bossDatabase = new DataDrivenBossDatabase(Path.Combine(Paths.PluginPath, "WeaverNet", "Data", "Bosses"));
                bossDatabase.Load();

                Logger.LogInfo("Configs Loaded Successfully");

                ITeleportService tpService = gameObject.AddComponent<TeleportService>();
                gameObject.AddComponent<HitboxVisualizer>();
                Logger.LogInfo("Services Initialized Successfully");

                var windowManager = gameObject.AddComponent<PluginWindowManager>();
                windowManager.Initialize(Config);

                var utilitiesWindow = new UtilitiesWindow("Utilities", tpService, bossDatabase);

                windowManager.Register(utilitiesWindow, true);

                Logger.LogInfo("GUI Initialized Successfully");

                Harmony.CreateAndPatchAll(typeof(CursorPatcher), null);

                Logger.LogInfo("Harmony Patching Successufll");

                Logger.LogInfo("Everything loaded successfully");
            }
            catch(Exception ex)
            {
                 PluginLog.Error(ex.Message);
            }
        }
        private void OnDestroy()
        {
            Logger.LogInfo("WeaverNet Mod shutdown.");
        }
    }
}