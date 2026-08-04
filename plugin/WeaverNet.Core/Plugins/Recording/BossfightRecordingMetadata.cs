using System;
using WeaverNet.Core.Game;
using WeaverNet.Core.Orchestration;

namespace WeaverNet.Core.Plugins.Recording
{
    public class BossfightRecordingMetadata
    {
        public Guid Id { get; }
        public string BossId { get; }
        public DateTime StartTime { get; }
        public Loadout Loadout { get; }
        public FightResult Result { get; }
        public string RecordingFormat { get; }
        public BossfightRecordingMetadata(Guid id, string bossId, DateTime startTime, Loadout loadout, FightResult result, string recordingFormat)
        {
            Id = id;
            BossId = bossId;
            StartTime = startTime;
            Loadout = loadout;
            Result = result;
            RecordingFormat = recordingFormat;
        }
    }
}
