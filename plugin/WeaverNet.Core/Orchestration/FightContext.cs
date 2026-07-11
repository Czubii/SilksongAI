using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;

namespace WeaverNet.Core.Orchestration
{
    public class FightContext
    {
        public Guid Id { get; } = Guid.NewGuid();
        public BossMetadata Boss { get; }
        public FightContext(BossMetadata targetBoss)
        {
            Boss = targetBoss ?? throw new ArgumentNullException(nameof(targetBoss));
        }
    }
}
