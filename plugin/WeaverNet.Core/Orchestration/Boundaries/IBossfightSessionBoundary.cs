namespace WeaverNet.Core.Orchestration.Boundaries
{
    public interface IBossfightSessionBoundary
    {
        bool ShouldTerminate(BossfightSessionRuntime runtime);
    }
}
