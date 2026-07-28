using System;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Popups
{
    public class NotificationPopup : BasePopup
    {
        private readonly string _message;
        private readonly string _title;
        public NotificationPopup(Rect parentRect, string Title, string message) :
        base(PopupLayer.Notification, parentRect, new Vector2(350, 120))
        {
            _title = Title;
            _message = message.ToString();
        }

        public override bool AlwaysVisible => false;

        protected override void DrawContent()
        {
            PluginGUI.BeginVertical();
            PluginGUI.Label(_title, WeaverNetStyles.PopupTitleLabel);

            GUILayout.FlexibleSpace();
            PluginGUI.Label(_message, WeaverNetStyles.PopupDescriptionLabel);
            GUILayout.FlexibleSpace();
            PluginGUI.BeginHorizontal();
            if (PluginGUI.Button("Ok"))
            {
                Close();
            }
            PluginGUI.EndHorizontal();
            PluginGUI.EndVertical();
        }
    }
}
