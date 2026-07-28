using UnityEngine;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Windows
{
    public class TestWindow : BaseWindow
    {
        private readonly IBossfightCatalog _catalog;
        private DropdownState<BossData> dropdownState = new DropdownState<BossData>();
        private Vector2 scroll = new Vector2();
        string text = "";
        bool toggle = false;
        public TestWindow(IBossfightCatalog catalog) : base("Test window", new Rect(100, 100, 100, 100))
        {
            _catalog = catalog;
        }
        public override bool CanEnable() => true;

        protected override void DrawContent()
        {
            scroll = PluginGUI.BeginScrollView(scroll);

            text = PluginGUI.Labeled(
                "Input some text", 
                () => PluginGUI.TextField(text),
                "the text will be used to do some very important things");

            text = PluginGUI.Labeled(
                "Input some text (no description)",
                () => PluginGUI.TextField(text));

            toggle = PluginGUI.Labeled(
                "Do you have Alzheimer's?",
                () => PluginGUI.Toggle(toggle),
                "(I Know you do)");

            dropdownState = PluginGUI.Labeled("Boss Selection",
                () => PluginGUI.Dropdown(Context, dropdownState, _catalog.Bosses, a => a.DisplayName));

            PluginGUI.EndScrollView();
            GUILayout.FlexibleSpace();
            PluginGUI.BeginHorizontal();
            PluginGUI.Button("Cancel");
            if (PluginGUI.Button("Accept"))
            {
                var confirmPopup = new Popups.ConfirmPopup(WindowRect, "Confirm Action", "this is very important", () => { PluginLog.Error("YOU CONFIRMED DEMISE"); });
                Context.ShowPopup(confirmPopup, this);
            }
            PluginGUI.EndHorizontal();

        }
    }
}
