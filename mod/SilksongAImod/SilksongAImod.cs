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
using Steamworks;
using HarmonyLib.Tools;

[BepInPlugin("com.czubii.SilksongAImod", "Silksong AI mod", "1.0.0 ")]
public class SilksongAImod : BaseUnityPlugin
{
    private ModGUI gui;
    public static ManualLogSource Log;
    public static string SteamUserName = "Unknown";
    public static bool GodModeEnabled { get; set; } = false;


    private void Awake()
    {
        Log = Logger;
        Log.LogInfo("Plugin loaded and initialized");

        gui = new ModGUI(this);

        HarmonyFileLog.Enabled = true;
        Harmony.CreateAndPatchAll(typeof(SilksongAImod), null);
        Harmony.CreateAndPatchAll(typeof(EnemyTracker), null);

    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SteamAPI), "Init")]
    private static void SteamAPIInitPostFix()
    {
        try
        {
            if (SteamAPI.IsSteamRunning())
            {
                SteamUserName = SteamFriends.GetPersonaName();
                SilksongAImod.Log.LogMessage($"Player name used for recording info: {SteamUserName}");
            }
        }
        catch (Exception e)
        {
            SilksongAImod.Log.LogWarning($"Steam name failed: {e}");
        }
    }




    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11))
        {
            GameStateLogger.LogGameStateToFiles();
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            BossFightRecordingSession.StopRecordingSession();
        }

        BossFightRecordingSession.Update();
    }

    private void OnGUI()
    {
        gui.OnGUIDrawStateLabel();
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

