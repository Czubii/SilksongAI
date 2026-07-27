using System;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Elements;
using WeaverNet.Mod.WeaverGUI.Styles;

namespace WeaverNet.Mod.WeaverGUI.Popups
{
    public class ConfirmPopup : BasePopup
    {
        private readonly Action _onConfirmed;
        private readonly Action _onCancelled;
        private readonly string _message;
        private readonly string _title;
        public ConfirmPopup(Rect parentRect, string Title, string message, Action OnConfirmed, Action OnCancelled = null) : 
            base(PopupLayer.Notification, parentRect, new Vector2(350,120))
        {
            _title = Title;
            _message = message;
            _onConfirmed = OnConfirmed;
            _onCancelled = OnCancelled;
        }
        protected override void DrawWindow(int id)
        {
            PluginGUI.BeginVertical();
            PluginGUI.Label(_title, WeaverNetStyles.PopupTitleLabel);

            GUILayout.FlexibleSpace();
            PluginGUI.Label(_message, WeaverNetStyles.PopupDescriptionLabel);
            GUILayout.FlexibleSpace();

            PluginGUI.BeginHorizontal();
            if (PluginGUI.Button("Cancel"))
            {
                _onCancelled?.Invoke();
                Close();
            }
            if (PluginGUI.Button("Confirm"))
            {
                _onConfirmed?.Invoke();
                Close();
            }
            PluginGUI.EndHorizontal();
            PluginGUI.EndVertical();
        }
    }
}
