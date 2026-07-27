using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Mod.Game
{
    public class BossfightSessionGameController: IBossfightSessionGameController
    {

        private readonly ILoadoutManager _loadoutManager;
        private readonly ITeleportService _teleportService;
        private readonly IBossSpawner _bossSpawner;
        private readonly ICombatEntityQuery _combatEntityQuery;
        public BossfightSessionGameController(
            ILoadoutManager loadoutManager,
            ITeleportService teleportService,
            IBossSpawner bossSpawner,
            ICombatEntityQuery combatEntityQuery)
        {
            _loadoutManager = loadoutManager;
            _teleportService = teleportService;
            _bossSpawner = bossSpawner;
            _combatEntityQuery = combatEntityQuery;
        }

        public void SelectLoadout(Loadout loaodut, TemporaryStateModifier modifier)
        {
            _loadoutManager.SetLoadoutTemporary(loaodut, modifier);
        }
        public void SelectRespawnPoint(IRespawnPoint spawnPoint, TemporaryStateModifier modifier)
        {
            spawnPoint.UseAsTemporary(modifier);
        }
        public void RespawnBoss(BossData boss, TemporaryStateModifier modifier)
        {
            _bossSpawner.RespawnTemporary(boss.RespawnFlags, modifier);
        }
        public Task TeleportToBossAsync(BossData boss)
        {
            return _teleportService.TeleportAsync(boss.ArenaSceneName, boss.ArenaPosition, boss.RequireHardSceneReload);
        }
        public Task TeleportToBenchAsync()
        {
            return _teleportService.TeleportToBenchAsync();
        }
        public async Task<int> WaitForBossAsync(BossData boss, CancellationToken ct)
        {
            var enemyInstance = await _combatEntityQuery.WaitForEnemyAsync(a => a.Name == boss.Id, ct);
            return enemyInstance.Id;
        }
        public async Task<FightResult> AwaitFightFinishedAsync(int bossGameObjectId, CancellationToken ct)
        {
            if(!_combatEntityQuery.TryGetEnemy(bossGameObjectId, out var bossInstance))
            {
                return FightResult.BossMissing;
            }

            var hm = bossInstance.HealthManager;

            while(true)
            {
                var pd = PlayerData.instance;

                if (pd == null)
                    return FightResult.HeroMissing;
                if (bossInstance == null || hm == null)
                    return FightResult.BossMissing; 
                if (hm.isDead)
                    return FightResult.Success;
                if (pd.health <= 0)
                    return FightResult.Failure;
                if (ct.IsCancellationRequested)
                    return FightResult.Cancelled;

                await Task.Yield();
            }
        }
    }
}
