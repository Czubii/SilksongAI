using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;
using WeaverNet.Mod.WeaverGUI.Views;

namespace WeaverNet.Mod.WeaverGUI
{
    public abstract class MultiViewWindow: __BaseWindow
    {
        private class ViewEntry
        {
            public string Title { get; }
            public Action Draw { get; }
            public ViewEntry(Action draw, string title)
            {
                Title = title;
                Draw = draw;
            }
        }

        private readonly Dictionary<string, ViewEntry> _views = new Dictionary<string, ViewEntry>();
        protected string CurrentView {  get; private set; }
        public MultiViewWindow(string name, Rect windowRect): base(name, windowRect) { }

        protected void AddView(string name, Action drawMethod, string title = null)
        {
            if (_views.ContainsKey(name))
                throw new ArgumentException($"View '{name}' already exists.");

            _views[name] = new ViewEntry(drawMethod, title);

            if (CurrentView == null)
                CurrentView = name;
        }
        protected void AddView(string name, IView view, string title = null)
        {
            if (_views.ContainsKey(name))
                throw new ArgumentException($"View '{name}' already exists.");

            _views[name] = new ViewEntry(view.Draw, title);
            view.ViewRequested += SwitchView;

            if (CurrentView == null)
                CurrentView = name;
        }
        protected void SwitchView(string name)
        {
            if (_views.ContainsKey(name))
                CurrentView = name;
        }
        public override void DrawContent()
        {
            if (CurrentView == null)
                return;

            if (_views.TryGetValue(CurrentView, out var view))
            {
                if(view.Title != null) GUILayout.Label(view.Title, WeaverNetStyles.ViewTitleLabel);

                view.Draw();
            }
        }

    }
}
