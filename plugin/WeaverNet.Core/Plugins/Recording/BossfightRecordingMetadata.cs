using System;
using WeaverNet.Core.Game;
using WeaverNet.Core.Orchestration;

namespace WeaverNet.Core.Plugins.Recording
{
    public class BossfightRecordingMetadata
    {
        public Guid Id { get; }
        public string RecordingFrameTypeId { get; }
        public int RecordingFrameTypeVersion { get; }
        public string BossId { get; }
        public DateTime StartTime { get; }
        public Loadout Loadout { get; }
        public FightResult Result { get; }
        //TODO  public int FrameCount { get; }
        public BossfightRecordingMetadata(
            Guid id, 
            string recordingFrameTypeId, 
            int recordingFrameTypeVersion, 
            string bossId, 
            DateTime startTime, 
            Loadout loadout, 
            FightResult result)
        {
            Id = id;
            BossId = bossId;
            StartTime = startTime;
            Loadout = loadout;
            Result = result;
            RecordingFrameTypeId = recordingFrameTypeId;
            RecordingFrameTypeVersion = recordingFrameTypeVersion;
        }
    }
}
