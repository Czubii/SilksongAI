using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AIPlugin.PluginGUI
{
    public abstract class BasePluginWindow: MonoBehaviour
    {
        public abstract Vector2 Size { get; }
        public abstract Vector2 Position { get; set; }
        public abstract bool EnableKeyDown();
    }

    public class PluginWindowManager : MonoBehaviour
    {
        private List<BasePluginWindow> _windows = new List<BasePluginWindow>();
        private readonly float _windowSpacing = 20.0f;
        private readonly float _windowPosY = 300.0f;
        public void Register(BasePluginWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("Widnow parameter cannot be null");

            if (!_windows.Contains(window))
                _windows.Add(window);

            UpdatePositions();
        }
        void Update()
        {
            foreach (var window in _windows)
            {
                if (window.EnableKeyDown())
                {
                    window.enabled = !window.enabled;
                    UpdatePositions();
                }
                    
            }
        }
        void UpdatePositions()
        {
            float posX = _windowSpacing;
            foreach (var window in _windows)
            {
                if (window.enabled) 
                {
                    window.Position = new Vector2(posX, _windowPosY);
                    posX += window.Size.x + _windowSpacing;
                }
            }
        }
    }
}
