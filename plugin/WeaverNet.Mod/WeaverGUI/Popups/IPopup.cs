using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Mod.WeaverGUI
{
    public interface IPopup
    {
        PopupLayer Layer { get; }
        bool IsAlive { get; }
        bool AlwaysVisible { get; }
        void BringToFront();
        void Initialize(IGUIContext context);
        void Render();
        void Close();
    }
}
