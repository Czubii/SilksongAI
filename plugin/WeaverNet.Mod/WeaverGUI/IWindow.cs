using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Mod.WeaverGUI
{
    public interface IWindow
    {
        string Name { get; }
        bool IsOpen { get; set; }
        bool ShowInToolbar { get; }
        bool CanEnable();
        void Initialize(IGUIContext context);
        void Render();
    }
}
