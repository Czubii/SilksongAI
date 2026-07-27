namespace WeaverNet.Core.Orchestration.Boundaries
{
    public interface IBossfightSessionBoundary
    {
        bool ShouldTerminate(BossfightSessionRuntime runtime);
        BossfightSessionProgress GetSessionProgress(BossfightSessionRuntime runtime);
    }
}
