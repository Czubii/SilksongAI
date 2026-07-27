using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Mod.WeaverGUI
{
    public interface IGUIContext
    {
        bool IsGUIVisible { get; }
        IWindow ActiveWindow { get; }
        int AllocateID();
        void SetActiveWindow(IWindow window);
        void ShowGUI();
        void HideGUI();
        void ShowOverlay(IGUIOverlay draw);
    }
}
