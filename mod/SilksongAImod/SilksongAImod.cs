using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GenericVariableExtension;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using SilksongAI;

[BepInPlugin("com.czubii.SilksongAImod", "Silksong AI mod", "1.0.0 ")]
public class SilksongAImod : BaseUnityPlugin
{
    private ModGUI gui;
    public static ManualLogSource Log;
    private BossFightRecorder bossFightRecorder;

    public static bool GodModeEnabled { get; set; } = false;


    private void Awake()
    {
        Log = Logger;
        Log.LogInfo("Plugin loaded and initialized");

        gui = new ModGUI(this);

        bossFightRecorder = new BossFightRecorder();

        Harmony.CreateAndPatchAll(typeof(SilksongAImod), null);

        
        

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            GameStateLogger.LogGameStateToFiles();
        }

        bossFightRecorder.RecordFrame();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerData), "TakeHealth")]
    private static void TakeHealthPostfix(PlayerData __instance, int amount, bool hasBlueHealth, bool allowFracturedMaskBreak)
    {
        if (GodModeEnabled)
        {
            __instance.health = __instance.maxHealth;
        }
    }


}

