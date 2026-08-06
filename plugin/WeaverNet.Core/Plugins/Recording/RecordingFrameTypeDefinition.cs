using System;

namespace WeaverNet.Core.Plugins.Recording
{
    public class RecordingFrameTypeDefinition
    {
        public string Id { get; }
        public int Version { get; }
        public Type FrameType { get; }
        public RecordingFrameTypeDefinition(string id, int version, Type frameType)
        {
            if (!frameType.IsClass) throw new ArgumentException("Frame must be a class type");

            Id = id;
            FrameType = frameType;
            Version = version;
        }
    }
}
