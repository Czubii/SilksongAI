using System.Collections.Generic;
using System.Linq;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Infrastructure.Interfaces;

namespace WeaverNET.Infrastructure.Catalogs
{
    /// <summary>
    /// Should be construted at startup and not disposed of until the application is shutting down.
    /// </summary>
    public class RecordingCatalog : IRecordingCatalog
    {
        private readonly IRecordingRepository _recordingRepository;
        private readonly IBossRepository _bossRepository;
        public IReadOnlyList<BossData> DistinctBosses { get; private set; }
        public IReadOnlyList<Loadout> DistinctLoadouts { get; private set; }
        public RecordingCatalog(IBossRepository bossRepository, IRecordingRepository recordingRepository)
        {
            _recordingRepository = recordingRepository;
            _recordingRepository.RepositoryChanged += Update;
            _bossRepository = bossRepository;

            Update(); // Initial update to populate fields
        }
        private void Update()
        {
            var distinctBossIds = _recordingRepository.GetDistinctBossIds().ToHashSet();
            DistinctBosses = _bossRepository.All.Where(boss => distinctBossIds.Contains(boss.Id)).ToList();
            DistinctLoadouts = _recordingRepository.GetDistinctLoadouts();
        }
    }
}
