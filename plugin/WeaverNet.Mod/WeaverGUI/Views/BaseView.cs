using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Mod.WeaverGUI.Views
{
    public abstract class BaseView: IView
    {
        private IGUIContext _context;
        protected IGUIContext Context
        {
            get
            {
                if (_context == null)
                    throw new InvalidOperationException($"View has not been initialized");

                return _context;
            }
        }
        private IViewHost _owner;
        protected Rect WindowRect => _owner.WindowRect;

        public event Action<string> ViewRequested;
        public abstract void DrawContent();
        public void Initialize(IGUIContext context, IViewHost owner)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (_context != null)
                throw new InvalidOperationException(
                    "Window has already been initialized");

            _context = context;
            _owner = owner;
        }
        protected void RequestView(string viewName)
        {
            ViewRequested?.Invoke(viewName);
        }
        protected void ShowPopup(IPopup popup, bool lockWindow = true)
        {
            Context.ShowPopup(popup, lockWindow ? _owner : null);
        }
    }
}
