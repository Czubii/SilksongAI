using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using WeaverNet.Core.DataCollection;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Core.Plugins;
using WeaverNet.Mod.DataCollection;
using WeaverNet.Mod.Game;
using WeaverNet.Mod.Game.Bosses;
using WeaverNet.Mod.Game.Debug;
using WeaverNet.Mod.Game.Player;
using WeaverNet.Mod.WeaverGUI;
using WeaverNet.Mod.WeaverGUI.Views;
using WeaverNet.Mod.WeaverGUI.Windows;
using WeaverNET.Infrastructure.Data;
using WeaverNET.Infrastructure.Data.Json;
using WeaverNET.Infrastructure.Databases;

namespace WeaverNet.Mod
{
    [BepInPlugin(
        "com.weavernet.mod",
        "WeaverNet Mod",
        "0.1.0"
    )]
    public class WeaverNetPlugin : BaseUnityPlugin
    {
        public sealed class RepositoryContainer
        {
            public IBossfightCatalog Catalog { get; }
            public IBossRepository BossRepository { get; }
            public ILoadoutRepository LoadoutRepository { get; }

            public RepositoryContainer(
                IBossfightCatalog catalog,
                IBossRepository bossRepository,
                ILoadoutRepository loadoutRepository)
            {
                Catalog = catalog;
                BossRepository = bossRepository;
                LoadoutRepository = loadoutRepository;
            }
        }

        public sealed class ServiceContainer
        {
            public ITeleportService TeleportService { get; }
            public HitboxVisualizer HitboxVisualizer { get; }
            public IBossfightSessionAssembler SessionAssembler { get; }
            public IBossfightSessionOrchestrator Orchestrator { get; }
            public IBossfightSessionStatus BossfightSessionStatus { get; }
            public LoadoutManager LoadoutManager { get; }
            public IResourceReplenisher ResourceManager { get; }
            public IRaycastScanner RaycastScanner { get; }
            public RaycastScannerVisualizer RaycastVisualizer {  get; }
            public ServiceContainer(
                ITeleportService teleportService,
                HitboxVisualizer hitboxVisualizer,
                IBossfightSessionAssembler sessionAssembler,
                IBossfightSessionOrchestrator orchestrator,
                IBossfightSessionStatus bossfightSessionStatus,
                LoadoutManager loadoutManager,
                IResourceReplenisher resourceManager,
                IRaycastScanner raycastScanner,
                RaycastScannerVisualizer raycastVisualizer)
            {
                TeleportService = teleportService;
                HitboxVisualizer = hitboxVisualizer;
                SessionAssembler = sessionAssembler;
                Orchestrator = orchestrator;
                BossfightSessionStatus = bossfightSessionStatus;
                LoadoutManager = loadoutManager;
                ResourceManager = resourceManager;
                RaycastScanner = raycastScanner;
                RaycastVisualizer = raycastVisualizer;
            }
        }

        private void Awake()
        {
            try
            {
                Logger.LogInfo("Loading WeaverNet Mod...");

                InitializeRuntime();

                var repositories = InitializePersistent();
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
            var logger = new BepInExPluginLogger(Logger);
            PluginLog.Bind(logger);

            PluginLog.Info("Runtime initialized.");
        }

        private RepositoryContainer InitializePersistent()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            PluginLog.Info("Loading repositories...");

            var bossRepoPath = Path.Combine(
                appDataPath,
                "WeaverNet",
                "Data",
                "Bosses");

            var loadoutRepoPath = Path.Combine(
                appDataPath,
                "WeaverNet",
                "Data",
                "Loadouts");

            var bossRepository = new JsonBossRepository(bossRepoPath);
            var loadoutRepository = new JsonLoadoutRepository(loadoutRepoPath);

            var catalog = new BossfightCatalog(
                bossRepository,
                loadoutRepository);

            PluginLog.Info("Repositories loaded successfully.");

            return new RepositoryContainer(
                catalog,
                bossRepository,
                loadoutRepository);
        }

        private ServiceContainer InitializeServices()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            PluginLog.Info("Initializing services...");

            var loadoutManager = new LoadoutManager();
            var bossSpawner = new BossSpawner();
            var teleportService = new TeleportService();
            var combatEntityTracker = new CombatEntityTracker();
            var resourceManager = new ResourceReplenisher();
            var raycastScanner = new HeroSurroundingsScanner();

            CombatEntityTrackerPatches.Initialize(combatEntityTracker);

            var hitboxVisualizer = gameObject.AddComponent<HitboxVisualizer>();
            var raycastVisualizer = gameObject.AddComponent<RaycastScannerVisualizer>();
            raycastVisualizer.Initialize(raycastScanner);

            var bossfightController = new BossfightSessionGameController(
                loadoutManager,
                resourceManager,
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


            var sessionTypeRegistry = new BossfightSessionTypeRegistry();
            var defaultSession = new DefaultBossfightSession();

            var recordingFrameSource = gameObject.AddComponent<FixedUpdateFrameSource>();

            var recordingFileWriter = new JsonFrameWriter();
            var recordingSession = new RecordingBossfightSession(Path.Combine(appDataPath, "WeaverNet","Recordings"), recordingFrameSource, recordingFileWriter);

            sessionTypeRegistry.Register(defaultSession);
            sessionTypeRegistry.Register(recordingSession);

            var assembler = new BossfightSessionAssembler(
                objectFactory,
                sessionTypeRegistry);

            PluginLog.Info("Services initialized successfully.");

            return new ServiceContainer(
                teleportService,
                hitboxVisualizer,
                assembler,
                orchestrator,
                bossfightSessionStatus,
                loadoutManager,
                resourceManager,
                raycastScanner,
                raycastVisualizer);
        }

        private void InitializeGUI(RepositoryContainer repositories, ServiceContainer services)
        {
            PluginLog.Info("Initializing GUI...");

            var windowManager = gameObject.AddComponent<GUIManager>();
            windowManager.Initialize(Config);

            var loadoutWindow = new LoadoutWindow(
                "Loadouts",
                services.LoadoutManager,
                repositories.LoadoutRepository);

            var setupView = new StandardBossfightSetupView(
                repositories.Catalog,
                services.LoadoutManager,
                services.SessionAssembler,
                services.Orchestrator,
                services.BossfightSessionStatus);

            var bossfightSessionWindow = new BossfightSessionWindow(
                "Bossfight Session",
                setupView);

            var debugWindow = new DebugWindow("Debug", services.HitboxVisualizer, services.RaycastVisualizer);

            windowManager.Register(loadoutWindow);
            windowManager.Register(bossfightSessionWindow);
            windowManager.Register(debugWindow);

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