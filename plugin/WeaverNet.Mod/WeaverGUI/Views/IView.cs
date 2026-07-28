using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Mod.WeaverGUI.Views
{
    public interface IView
    {
        event Action<string> ViewRequested;
        void Initialize(IGUIContext context, IViewHost owner);
        void DrawContent();
    }
}
    