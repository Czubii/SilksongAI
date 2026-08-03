using WeaverNet.Core.Orchestration;

namespace WeaverNET.Infrastructure.Databases
{
    public class RecordingMetadata
    {
        public FightContext FightContext { get; }
        public RecordingMetadata(FightContext fightContext)
        {
            FightContext = fightContext;
        }
    }
}
