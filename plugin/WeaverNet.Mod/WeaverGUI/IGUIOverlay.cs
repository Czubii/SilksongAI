using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Mod.WeaverGUI
{
    public interface IGUIOverlay
    {
        bool IsAlive { get; }
        void Draw();
    }
}
