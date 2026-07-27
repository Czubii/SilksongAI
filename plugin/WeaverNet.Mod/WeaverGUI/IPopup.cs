using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Mod.WeaverGUI
{
    public interface IPopup
    {
        int ID { get; }
        PopupLayer Layer { get; }
        bool IsOpen { get; }
        void AssignID(int id);
        void Render();
        void Close();
    }
}
