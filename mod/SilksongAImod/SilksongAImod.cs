using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GenericVariableExtension;
using HarmonyLib;
using HutongGames.PlayMaker.Actions;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[BepInPlugin("com.yourname.SilksongAImod", "Silksong AI mod", "1.0.0 ")]
public class SilksongAImod : BaseUnityPlugin
{
    private ConfigEntry<bool> getPositionButton;
    private ConfigEntry<bool> printEnemiesButton;
    private ConfigEntry<bool> godModeToggle;
    private ConfigEntry<bool> fightMossMotherButton;
    internal static ManualLogSource Log;

    private static bool godMode = false;

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


        getPositionButton = Config.Bind(
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

        godModeToggle = Config.Bind(
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

        fightMossMotherButton = Config.Bind(
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

        printEnemiesButton = Config.Bind(
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

        SceneManager.sceneLoaded += OnSceneLoaded;
        


        Harmony.CreateAndPatchAll(typeof(SilksongAImod), null);


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

    private void DrawGodModeToggle(ConfigEntryBase entry)
    {
        godMode = GUILayout.Toggle(godMode, "GodMode");
    }

    private void DrawPrintEnemiesButton(ConfigEntryBase entry)
    {
        if (GUILayout.Button("Log Print Enemies"))
        {
            foreach (var hm in FindObjectsByType<HealthManager>(FindObjectsSortMode.None))
            {
                Logger.LogInfo(hm.gameObject.name);
            }
        }
    }

    private void DrawFightMossMotherButton(ConfigEntryBase entry)
    {
        if (GUILayout.Button("Fight Moss Mother"))
        {
            RespawnMossMother();

            string scene_name = "Tut_03";
            Vector3 pos = new Vector3(68f, 17.6f, 0);
            TeleportTo(scene_name, pos);
        }
    }

    private void RespawnMossMother()
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

    private void TeleportTo(string sceneName, Vector3 pos)
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
        if (godMode)
        {
            __instance.health = __instance.maxHealth;
        }
    }


}

