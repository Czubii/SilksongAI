namespace WeaverNet.Core.Orchestration.Boundaries
{
    public class IterationBoundary: IBossfightSessionBoundary
    {
        public int MaxIter { get; }

        IterationBoundary(int maxIter)
        {
            MaxIter = maxIter;
        }
        public bool ShouldTerminate(BossfightSessionRuntime runtime)
        {
            return runtime.CurrentIteration >= MaxIter;
        }
    }
}
