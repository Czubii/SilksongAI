using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    internal static class SessionConfig
    {
        public enum RecordingOutputTypes
        {
            JSON, // Only for debugging. Python wont be able to load those recording as of now
            MSGPACK
        }
        public static readonly int RecordFrameDelta = 10;
        public static readonly RecordingOutputTypes RecordingOutputType = RecordingOutputTypes.MSGPACK;
        public static readonly int AwaitBossTimeoutFrames = 2000;
    }
}
