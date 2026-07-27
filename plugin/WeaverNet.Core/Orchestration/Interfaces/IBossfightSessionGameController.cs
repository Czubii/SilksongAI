using System.Threading;
using System.Threading.Tasks;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionGameController
    {
        void SelectLoadout(Loadout loadout, TemporaryStateModifier modifier);
        void SelectRespawnPoint(IRespawnPoint spawnPoint, TemporaryStateModifier modifier);
        void RespawnBoss(BossData boss, TemporaryStateModifier modifier);
        Task TeleportToBossAsync(BossData boss);
        Task TeleportToBenchAsync();
        Task<int> WaitForBossAsync(BossData boss, CancellationToken ct);
        Task<FightResult> AwaitFightFinishedAsync(int bossGameObjectId, CancellationToken ct);
    }
}
