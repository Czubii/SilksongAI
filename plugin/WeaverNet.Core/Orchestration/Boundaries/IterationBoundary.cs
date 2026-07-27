namespace WeaverNet.Core.Orchestration.Boundaries
{
    public class IterationBoundary: IBossfightSessionBoundary
    {
        public int MaxIter { get; }

        public IterationBoundary(int maxIter)
        {
            MaxIter = maxIter;
        }
        public bool ShouldTerminate(BossfightSessionRuntime runtime)
        {
            return runtime.CurrentIteration >= MaxIter;
        }
        public BossfightSessionProgress GetSessionProgress(BossfightSessionRuntime runtime)
        {
            return new BossfightSessionProgress(
                $"Iteration {runtime.CurrentIteration+1}/{MaxIter}",
                (float)(runtime.CurrentIteration+1)/(float)MaxIter //TODO fix that smelly iteration+1
                );
        }
    }
}
