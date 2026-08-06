using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.DataCollection
{
    public class FixedUpdateEventSource : MonoBehaviour, IFixedUpdateEventSource
    {
        private readonly List<IFixedUpdateListener> _listeners = new List<IFixedUpdateListener>();

        public void Register(IFixedUpdateListener listener)
        {
            if (_listeners.Contains(listener))
                return;
            _listeners.Add(listener);
        }

        public void Unregister(IFixedUpdateListener listener)
        {
            _listeners.Remove(listener);
        }
        private void FixedUpdate()
        {
            foreach (var listener in _listeners)
            {
                try
                {
                    listener.OnFixedUpdate();
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Error occured when notifying FixedUpdate: {ex}");
                }
            }
        }
    }
}
