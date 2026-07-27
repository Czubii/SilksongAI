using System.Collections.Generic;
using System.Linq;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure.Interfaces;

namespace WeaverNET.Infrastructure.Data
{
    public class BossfightCatalog : IBossfightCatalog
    {
     
        private readonly IBossRepository _bossRepository;
        private readonly ILoadoutRepository _loadoutRepository;

        public IReadOnlyList<BossData> Bosses { get; private set; }
        public IReadOnlyList<Loadout> Loadouts { get; private set; }

        public BossfightCatalog(
            IBossRepository bossRepository,
            ILoadoutRepository loadoutRepository)
        {
            _bossRepository = bossRepository;
            _loadoutRepository = loadoutRepository;

            Bosses = _bossRepository.All.ToList();

            _loadoutRepository.RepositoryChanged += UpdateLoadouts;

            UpdateLoadouts();
        }

        private void UpdateLoadouts()
        {
            Loadouts = _loadoutRepository.All.ToList();
        }
    }
}
