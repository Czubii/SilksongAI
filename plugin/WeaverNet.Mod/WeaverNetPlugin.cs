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
using WeaverNet.Core.Plugins.FrameCapture;
using WeaverNet.Core.Plugins.Recording;
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
using WeaverNET.Infrastructure.Recording;

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
            public IRecordingRepository RecordingRepository { get; }
            public RepositoryContainer(
                IBossfightCatalog catalog,
                IBossRepository bossRepository,
                ILoadoutRepository loadoutRepository,
                IRecordingRepository recordingRepository)
            {
                Catalog = catalog;
                BossRepository = bossRepository;
                LoadoutRepository = loadoutRepository;
                RecordingRepository = recordingRepository;
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
                var services = InitializeServices(repositories);

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

            var recordingRepoPath = Path.Combine(
                appDataPath,
                "WeaverNet",
                "Recordings"
                );

            var bossRepository = new JsonBossRepository(bossRepoPath);
            var loadoutRepository = new JsonLoadoutRepository(loadoutRepoPath);
            var recordingRepository = new RecordingRepository(recordingRepoPath);

            var catalog = new BossfightCatalog(
                bossRepository,
                loadoutRepository);

            PluginLog.Info("Repositories loaded successfully.");

            return new RepositoryContainer(
                catalog,
                bossRepository,
                loadoutRepository,
                recordingRepository);
        }

        private ServiceContainer InitializeServices(RepositoryContainer repositories)
        {
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

            var sessionTypeRegistry = InitializeSessionTypes(repositories, combatEntityTracker);

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
        private BossfightSessionTypeRegistry InitializeSessionTypes(RepositoryContainer repositories, ICombatEntityQuery entityQuery)
        {
            var tempDirectory = Path.GetTempPath();
            var sessionTypeRegistry = new BossfightSessionTypeRegistry();
            var fixedUpdateSource = gameObject.AddComponent<FixedUpdateEventSource>();
            // Default session ----------
            var defaultSession = new DefaultBossfightSession("default");

            // Recordings ---------------
            var recordingFrameTypeRegistry = new RecordingFrameTypeRegistry();

            var behavioralCloningFrameType =
                new RecordingFrameTypeDefinition(
                    "behavioral-cloning",
                    BCFrame.FormatVersion,
                    typeof(BCFrame));

            recordingFrameTypeRegistry.Register(behavioralCloningFrameType);

            // Recording. Here is a nice example of how to capture any data using this generic recording pipeline
            var frameGenerator = new BCFrameGenerator(entityQuery);
            var recordingFrameSourceFactory = new FixedUpdateFrameSourceFactory<BCFrame>(frameGenerator, fixedUpdateSource);

            // You can select Msgpack / JSON output:
            IRecordingParser recordingFileWriter = new JsonRecordingParser();
            //IRecordingWriter<BehavioralCloningFrame> recordingFileWriter = new MsgpackFrameWriter<BehavioralCloningFrame>();

            // The session factory implements the service wiring:
            var recordingSession = new RecordingBossfightSessionFactory<BCFrame>(
                "behavioral-cloning-recording",
                Path.Combine(tempDirectory, "WeaverNet", "Recordings"),
                behavioralCloningFrameType,
                recordingFrameSourceFactory,
                recordingFileWriter,
                repositories.RecordingRepository);


            // Registration ------------
            sessionTypeRegistry.Register(defaultSession);
            sessionTypeRegistry.Register(recordingSession);

            return sessionTypeRegistry;
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