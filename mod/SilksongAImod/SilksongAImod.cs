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


public struct EnemyData
{

    public Vector3 pos;
    public Vector2 vel;
    public int hp;
    public PlayMakerFSM fsm;

}

public struct HeroData
{
    Vector3 pos;
    Vector3 vel;
    int hp;
}


[BepInPlugin("com.czubii.SilksongAImod", "Silksong AI mod", "1.0.0 ")]
public class SilksongAImod : BaseUnityPlugin
{
    private ModGUI gui;
    public static ManualLogSource Log;

    public static bool GodModeEnabled { get; set; } = false;


private void Awake()
    {
        Log = Logger;
        Log.LogInfo("Plugin loaded and initialized");

        gui = new ModGUI(this);

        SceneManager.sceneLoaded += TeleportUtils.OnSceneLoaded;
        
        Harmony.CreateAndPatchAll(typeof(SilksongAImod), null);


    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= TeleportUtils.OnSceneLoaded;
    }

  

    

    public void RespawnMossMother()
    {
        PlayerData.instance.defeatedMossMother = false;
        SceneData.instance.PersistentBools.SetValue(new PersistentItemData<bool>
        {
            SceneName = "Tut_03",
            ID = "Battle Scene",
            Value = false
        });
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

