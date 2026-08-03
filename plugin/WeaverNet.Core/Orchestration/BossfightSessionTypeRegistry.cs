using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionTypeRegistry
    {
        private readonly Dictionary<string, IBossfightSessionType> _types = new Dictionary<string, IBossfightSessionType>();
        public IReadOnlyDictionary<string, IBossfightSessionType> Types => _types;
        public void Register(IBossfightSessionType sessionType)
        {
            if (_types.ContainsKey(sessionType.Type)) throw new InvalidOperationException($"Session type '{sessionType.Type}' already exists");

            _types.Add(sessionType.Type, sessionType);
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
