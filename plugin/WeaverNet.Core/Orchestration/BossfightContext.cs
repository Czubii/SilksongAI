using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightContext
    {
        public BossData Boss { get; }
        public Loadout Loadout { get; }
        public DateTime StartTime { get; }
        public Guid Id { get; }
        public int CurrentIteration { get; }
        public BossfightContext(BossData boss, Loadout loadout, DateTime startTime, Guid id, int currentIteration)
        {
            Boss = boss;
            Loadout = loadout;
            StartTime = startTime;
            Id = id;
            CurrentIteration = currentIteration;
        }
    }
}
