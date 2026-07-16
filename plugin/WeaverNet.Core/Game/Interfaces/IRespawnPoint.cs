using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface IRespawnPoint: IDisposable
    {
        void UseAsTemporary(TemporaryStateModifier modifier, int type = 0);
    }
}
