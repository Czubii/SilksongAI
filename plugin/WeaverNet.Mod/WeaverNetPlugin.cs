using BepInEx;
using HarmonyLib;
using UnityEngine;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.PluginGUI;
using WeaverNet.Core.PluginGUI.Windows;
using WeaverNet.PluginGUI;

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
            Logger.LogInfo("WeaverNet Mod loading...");

            PluginRuntime.Initialize();
            var logger = new BepInExPluginLogger(Logger);
            PluginLog.Bind(logger);

            var windowManager = gameObject.AddComponent<PluginWindowManager>();
            windowManager.Initialize(Config);

            var utilitiesWindow = new UtilitiesWindow("Utilities");

            windowManager.Register(utilitiesWindow, true);

            Logger.LogInfo("Patching started...");
            Harmony.CreateAndPatchAll(typeof(CursorPatcher), null);
            Logger.LogInfo("Everything loaded successfully");
        }

        private void OnDestroy()
        {
            Logger.LogInfo("WeaverNet Mod shutdown.");
        }
    }
}