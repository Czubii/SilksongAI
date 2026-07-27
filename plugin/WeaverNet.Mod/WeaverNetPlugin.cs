using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Mod.Game;
using WeaverNet.Mod.Game.Bosses;
using WeaverNet.Mod.Game.Debug;
using WeaverNet.Mod.Game.Player;
using WeaverNet.Mod.WeaverGUI;
using WeaverNet.Mod.WeaverGUI.Styles;
using WeaverNet.Mod.WeaverGUI.Views;
using WeaverNet.Mod.WeaverGUI.Windows;
using WeaverNET.Infrastructure.Data;
using WeaverNET.Infrastructure.Data.Json;

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
                Logger.LogInfo("Loading WeaverNet Mod...");

                InitializeRuntime();

                var repositories = InitializeRepositories();
                var services = InitializeServices();

                InitializeGUI(repositories, services);
                InitializeHarmony();

                PluginLog.Info("WeaverNet Mod loaded successfully.");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to load WeaverNet Mod:\n{ex}");
            }
        }

        private void InitializeRuntime()
        {
            PluginRuntime.Initialize();

            var logger = new BepInExPluginLogger(Logger);
            PluginLog.Bind(logger);

            PluginLog.Info("Runtime initialized.");
        }

        private (
            IBossfightCatalog Catalog,
            IBossRepository BossRepository,
            ILoadoutRepository LoadoutRepository
        ) InitializeRepositories()
        {
            PluginLog.Info("Loading repositories...");

            var bossRepoPath = Path.Combine(
                Paths.PluginPath,
                "WeaverNet",
                "Data",
                "Bosses");

            var loadoutRepoPath = Path.Combine(
                Paths.PluginPath,
                "WeaverNet",
                "Data",
                "Loadouts");

            var bossRepository = new JsonBossRepository(bossRepoPath);
            var loadoutRepository = new JsonLoadoutRepository(loadoutRepoPath);

            var catalog = new BossfightCatalog(
                bossRepository,
                loadoutRepository);

            PluginLog.Info("Repositories loaded successfully.");

            return (
                catalog,
                bossRepository,
                loadoutRepository);
        }

        private (
            ITeleportService TeleportService,
            HitboxVisualizer HitboxVisualizer,
            IBossfightSessionAssembler SessionAssembler,
            IBossfightSessionOrchestrator Orchestrator,
            IBossfightSessionStatus BossfightSessionStatus,
            LoadoutManager LoadoutManager
        ) InitializeServices()
        {
            PluginLog.Info("Initializing services...");

            var loadoutManager = new LoadoutManager();
            var bossSpawner = new BossSpawner();
            var teleportService = new TeleportService();
            var combatEntityTracker = new CombatEntityTracker();
            CombatEntityTrackerPatches.Initialize(combatEntityTracker);

            var hitboxVisualizer = gameObject.AddComponent<HitboxVisualizer>();

            var bossfightController = new BossfightSessionGameController(
                loadoutManager,
                teleportService,
                bossSpawner,
                combatEntityTracker);

            var bossfightSessionStatus = new BossfightSessionStatus();

            var orchestrator = new BossfightSessionOrchestrator(
                bossfightController,
                bossfightSessionStatus);

            var respawnPointFactory = new RespawnPointFactory();
            var objectFactory = new BossfightSessionObjectFactory(
                respawnPointFactory);

            var assembler = new BossfightSessionAssembler(
                objectFactory);

            PluginLog.Info("Services initialized successfully.");

            return (
                teleportService,
                hitboxVisualizer,
                assembler,
                orchestrator,
                bossfightSessionStatus,
                loadoutManager);
        }

        private void InitializeGUI(
            (
                IBossfightCatalog Catalog,
                IBossRepository BossRepository,
                ILoadoutRepository LoadoutRepository
            ) repositories,
            (
                ITeleportService TeleportService,
                HitboxVisualizer HitboxVisualizer,
                IBossfightSessionAssembler SessionAssembler,
                IBossfightSessionOrchestrator Orchestrator,
                IBossfightSessionStatus BossfightSessionStatus,
                LoadoutManager LoadoutManager
            ) services)
        {
            PluginLog.Info("Initializing GUI...");

            var windowManager = gameObject.AddComponent<GUIManager>();
            windowManager.Initialize(Config);


            var testWindow = new TestWindow(repositories.Catalog);
            var testWindow2 = new TestWindow(repositories.Catalog);
            var testWindow3 = new TestWindow(repositories.Catalog);
            windowManager.Register(testWindow);
            windowManager.Register(testWindow2);
            windowManager.Register(testWindow3);

            //var utilitiesWindow = new UtilitiesWindow(
            //    "Utilities",
            //    services.TeleportService,
            //    repositories.BossRepository);

            //var loadoutWindow = new LoadoutWindow(
            //    "Loadouts",
            //    services.LoadoutManager,
            //    repositories.LoadoutRepository);

            //var setupView = new StandardBossfightSetupView(
            //    repositories.Catalog,
            //    services.SessionAssembler,
            //    services.Orchestrator);

            //var bossfightSessionWindow = new BossfightSessionWindow(
            //    "Bossfight Session",
            //    services.Orchestrator,
            //    services.BossfightSessionStatus,
            //    setupView);

            //var debugWindow = new DebugWindow(
            //    "Debug",
            //    services.HitboxVisualizer);

            //windowManager.Register(
            //    utilitiesWindow,
            //    true);

            //windowManager.Register(
            //    loadoutWindow,
            //    true);

            //windowManager.Register(
            //    bossfightSessionWindow,
            //    true);

            //windowManager.Register(
            //    debugWindow,
            //    true);

            PluginLog.Info("GUI initialized successfully.");
        }

        private void InitializeHarmony()
        {
            PluginLog.Info("Applying Harmony patches...");

            Harmony.CreateAndPatchAll(
                typeof(CursorPatches),
                null);
            Harmony.CreateAndPatchAll(
                typeof(CombatEntityTrackerPatches),
                null);

            PluginLog.Info("Harmony patches applied successfully.");
        }

        private void OnDestroy()
        {
            PluginLog.Info("WeaverNet Mod shutdown.");
        }
    }
}