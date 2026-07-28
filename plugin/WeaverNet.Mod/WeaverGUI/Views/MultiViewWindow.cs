using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Mod.WeaverGUI.Styles;
using WeaverNet.Mod.WeaverGUI.Views;

namespace WeaverNet.Mod.WeaverGUI
{
    public abstract class MultiViewWindow: BaseWindow, IViewHost
    {
        private class DrawActionEntry
        {
            public string Title { get; }
            public Action Draw { get; }
            public DrawActionEntry(Action draw, string title)
            {
                Title = title;
                Draw = draw;
            }
        }

        private readonly Dictionary<string, DrawActionEntry> _drawActions = new Dictionary<string, DrawActionEntry>();
        private readonly List<IView> _views = new List<IView>();
        protected string CurrentView {  get; private set; }

        public MultiViewWindow(string name, Rect windowRect): base(name, windowRect) { }

        protected void AddView(string name, Action drawMethod, string title = null)
        {
            if (_drawActions.ContainsKey(name))
                throw new ArgumentException($"View '{name}' already exists.");

            _drawActions[name] = new DrawActionEntry(drawMethod, title);

            if (CurrentView == null)
                CurrentView = name;
        }
        protected void AddView(string name, IView view, string title = null)
        {
            if (_drawActions.ContainsKey(name))
                throw new ArgumentException($"View '{name}' already exists.");

            _views.Add(view);
            _drawActions[name] = new DrawActionEntry(view.DrawContent, title);
            view.ViewRequested += SwitchView;

            if (CurrentView == null)
                CurrentView = name;
        }
        protected void SwitchView(string name)
        {
            if (_drawActions.ContainsKey(name))
                CurrentView = name;
        }

        public override void Initialize(IGUIContext context)
        {
            base.Initialize(context);

            foreach(var view in _views)
            {
                view.Initialize(context, this);
            }
        }

        protected override void DrawContent()
        {
            if (CurrentView == null)
                return;

            if (_drawActions.TryGetValue(CurrentView, out var view))
            {
                if(view.Title != null) GUILayout.Label(view.Title, WeaverNetStyles.ViewTitleLabel);

                view.Draw();
            }
        }

    }
}
