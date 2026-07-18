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
using WeaverNet.Mod.Game.Debug;
using WeaverNet.Mod.Game.Player;
using WeaverNet.Mod.WeaverGUI;
using WeaverNet.Mod.WeaverGUI.Windows;
using WeaverNET.Infrastructure.Data;

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

                var bossRepoPath = Path.Combine(Paths.PluginPath, "WeaverNet", "Data", "Bosses");
                JsonBossRepository bossRepo = new JsonBossRepository(bossRepoPath);

                var loadoutRepoPath = Path.Combine(Paths.PluginPath, "WeaverNet", "Data", "Loadouts");
                JsonLoadoutRepository loadoutRepo = new JsonLoadoutRepository(loadoutRepoPath);

                Logger.LogInfo("Configs Loaded Successfully");

                var loadoutManager = new LoadoutManager();

                ITeleportService tpService = gameObject.AddComponent<TeleportService>();
                var hitboxVisualizer = gameObject.AddComponent<HitboxVisualizer>();
                Logger.LogInfo("Services Initialized Successfully");

                var windowManager = gameObject.AddComponent<PluginWindowManager>();
                windowManager.Initialize(Config);

                var utilitiesWindow = new UtilitiesWindow("Utilities", tpService, bossRepo);
                var loadoutWindow = new LoadoutWindow("Loadouts", loadoutManager, loadoutRepo);
                var bossfightSessionWindow = new BossfightSessionWindow("Bossfight Session", bossRepo, loadoutRepo);
                var debugWindow = new DebugWindow("Debug", hitboxVisualizer);


                windowManager.Register(utilitiesWindow, true);
                windowManager.Register(loadoutWindow, true);
                windowManager.Register(bossfightSessionWindow, true);
                windowManager.Register(debugWindow, true);

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