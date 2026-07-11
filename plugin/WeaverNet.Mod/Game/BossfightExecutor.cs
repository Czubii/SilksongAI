using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Mod.Game
{
    public class BossfightExecutor : IBossfightExecutor
    {
        public async Task AwaitFightStartAsync(string bossID, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public void PreparePlayer(IAbilitySet abilitySet, ICrestToolSet toolSet, TemporaryStateModifier modifier, CancellationToken ct)
        {
            abilitySet.ApplyTemporary(modifier);
            toolSet.ApplyTemporary(modifier);
        }

        public async Task RespawnBossAsync(IBossBehavior bossBehavior, TemporaryStateModifier modifier, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task TeleportToArenaAsync(string arenaSceneName, Vector3 arenaPosition, bool requireSceneReload, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
