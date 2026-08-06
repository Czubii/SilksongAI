using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionTypeRegistry
    {
        private readonly Dictionary<string, IBossfightSessionPluginFactory> _types = new Dictionary<string, IBossfightSessionPluginFactory>();
        public IReadOnlyDictionary<string, IBossfightSessionPluginFactory> Types => _types;
        public void Register(IBossfightSessionPluginFactory sessionType)
        {
            if (_types.ContainsKey(sessionType.Id)) throw new InvalidOperationException($"Session type '{sessionType.Id}' already exists");

            _types.Add(sessionType.Id, sessionType);
        }

        public bool TryCreate(string type, BossfightSessionConfiguration context, out IReadOnlyList<IBossfightSessionPlugin> plugins)
        {
            if (_types.TryGetValue(type, out var sessionType))
            {
                plugins = sessionType.CreatePlugins(context);
                return true;
            }

            plugins = null;
            return false;
        }
    }
}
