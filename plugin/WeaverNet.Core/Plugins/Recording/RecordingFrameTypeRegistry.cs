using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WeaverNet.Core.Plugins.Recording
{
    public class RecordingFrameTypeRegistry : IRecordingFrameTypeQuery
    {
        private readonly Dictionary<string, RecordingFrameTypeDefinition> _all = new Dictionary<string, RecordingFrameTypeDefinition>();

        public void Register(RecordingFrameTypeDefinition definition)
        {
            if (_all.ContainsKey(definition.Id)) throw new InvalidOperationException($"Recording type '{definition.Id}' already registered!");
            _all.Add(definition.Id, definition);
        }
        public bool TryGet(string id, out RecordingFrameTypeDefinition definition)
        {
            return _all.TryGetValue(id, out definition);
        }
    }
}
