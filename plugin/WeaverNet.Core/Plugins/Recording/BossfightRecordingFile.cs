using System;
using System.IO;

namespace WeaverNet.Core.Plugins.Recording
{
    public class BossfightRecordingFile
    {
        public BossfightRecordingMetadata Metadata { get; }
        public FileInfo RecordingFile { get; }
        public string FileExtension { get; }
        public BossfightRecordingFile(
            BossfightRecordingMetadata metadata,
            FileInfo recordingFile,
            string fileExtension)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            RecordingFile = recordingFile ?? throw new ArgumentNullException(nameof(recordingFile));
            FileExtension = fileExtension;
        }
    }
}
