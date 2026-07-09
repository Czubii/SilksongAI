using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Infrastructure
{
    public class GameStateScope : IDisposable
    {
        public TemporaryStateModifier Modifier { get; }
        public GameStateScope()
        {
            Modifier = new TemporaryStateModifier();
        }

        public void Dispose()
        {
            Modifier.RestoreState();
        }
    }
}
