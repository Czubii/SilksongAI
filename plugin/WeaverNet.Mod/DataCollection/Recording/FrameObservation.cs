using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Mod.DataCollection.Recording;

namespace WeaverNet.Core.Plugins.Recording
{
    public class FrameObservation
    {
        public EnemyObservation Boss { get; }
        public FrameObservation(EnemyObservation boss)
        {
            Boss = boss;
        }
    }
}
