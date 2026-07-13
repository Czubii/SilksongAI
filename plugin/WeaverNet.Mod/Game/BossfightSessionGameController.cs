using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Mod.Game
{
    public class BossfightSessionGameController: IBossfightSessionGameController
    {
        public Task SelectAbilities(AbilitySet abilitySet, TemporaryStateModifier modifier)
        {
            abilitySet.ApplyTemporary(modifier);
            return Task.CompletedTask;
        }
        public Task SelectTools(CrestToolSet toolSet, TemporaryStateModifier modifier)
        {
            toolSet.ApplyTemporary(modifier);
            return Task.CompletedTask;
        }
        public Task SetRespawnPoint(IRespawnPoint spawnPoint, TemporaryStateModifier modifier)
        {
            spawnPoint.UseAsTemporary(modifier);
            return Task.CompletedTask;
        }
    }
}
