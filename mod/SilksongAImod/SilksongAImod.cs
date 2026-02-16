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
    internal static ManualLogSource Log;

    public static bool GodModeEnabled { get; set; } = false;

    private struct EnemyData
    {

        public Vector3 pos;
        public Vector2 vel;
        public int hp;
        public PlayMakerFSM fsm;

    }

    private struct HeroData
    {
        Vector3 pos;
        Vector3 vel;
        int hp;


    }

private void Awake()
    {
        Log = Logger;
        Log.LogInfo("Plugin loaded and initialized");

        gui = new ModGUI(this);

        SceneManager.sceneLoaded += OnSceneLoaded;
        


        Harmony.CreateAndPatchAll(typeof(SilksongAImod), null);


    }


   

    private void Update()
    {
        //Log.LogInfo($"Scene: CHUJ!");
        //Time.timeScale = 2f;
 
    }

    private EnemyData? getEnemyData(string enemyName)
    {
        var enemy = GameObject.Find(enemyName);
        if (enemy == null) return null;

        var hm = enemy.GetComponent<HealthManager>();
        if (hm == null) return null;

        var fsm = enemy.GetComponent<PlayMakerFSM>();
        if (fsm == null) return null;

        var rigidbody = enemy.GetComponent<Rigidbody2D>();
        if(rigidbody == null) return null;

        EnemyData output = new EnemyData();

        output.pos = enemy.transform.position;
        output.vel = rigidbody.linearVelocity;
        output.hp = hm.hp;
        output.fsm = fsm;


        return output;
    }

    private Vector3 getEnemyPosition(string enemyName)
    {
        var enemy = GameObject.Find(enemyName);
        if (enemy != null)
        {
            return enemy.transform.position;
        }
        return Vector3.zero;
    }


    private void debugPrintGameObjectComponents(string gameObjectName)
    {
        var obj = GameObject.Find(gameObjectName);
        if (obj == null) return;
        int numComponentes = obj.GetComponentCount();
        Log.LogInfo($"Num components: {numComponentes}");
        for (int i = 0; i < numComponentes; i++)
            Log.LogInfo(obj.GetComponentAtIndex(i));
    }

   private PlayMakerFSM[] getEnemyFSM(string enemyName)
    {
        var enemy = GameObject.Find(enemyName);
        if (enemy != null)
        {
            var fsms = enemy.GetComponents<PlayMakerFSM>();
            return fsms;
        }

        return null;
        
    }

    private int getEnemyHP(string enemyName)
    {
        int hp = -1;

        var enemy = GameObject.Find(enemyName);
        if (enemy != null)
        {
            var hm = enemy.GetComponent<HealthManager>();
            hp = hm.hp;
        }

        return hp;
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

    private Vector3 pendingPos;
    private string pendingScene;
    private bool pendingTeleport;

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void TeleportTo(string sceneName, Vector3 pos)
    {
        pendingScene = sceneName;
        pendingPos = pos;
        pendingTeleport = true;

        GameManager.instance.ChangeToScene(sceneName, "top1", 0.5f);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!pendingTeleport) return;
        if (scene.name != pendingScene) return;

        pendingTeleport = false;

        HeroController.instance.transform.position = pendingPos;
        GameManager.instance.cameraCtrl.PositionToHeroInstant(false);
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

