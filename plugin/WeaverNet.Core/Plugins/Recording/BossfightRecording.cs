using System;
using System.IO;

namespace WeaverNet.Core.Plugins.Recording
{
    public class BossfightRecording
    {
        public BossfightRecordingMetadata Metadata { get; }
        public FileInfo RecordingFile { get; }
        public BossfightRecording(
            BossfightRecordingMetadata metadata,
            FileInfo recordingFile)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            RecordingFile = recordingFile ?? throw new ArgumentNullException(nameof(recordingFile));
        }
    }
}
