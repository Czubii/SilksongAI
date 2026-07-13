using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionGameController
    {
        Task SelectAbilities(AbilitySet abilitySet, TemporaryStateModifier modifier);
        Task SelectTools(CrestToolSet toolSet, TemporaryStateModifier modifier);
        Task SetRespawnPoint(IRespawnPoint spawnPoint, TemporaryStateModifier modifier);
    }
}
