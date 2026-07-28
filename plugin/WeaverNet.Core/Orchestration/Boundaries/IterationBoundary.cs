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
                $"Completed Fights: {runtime.CurrentIteration}/{MaxIter}",
                (float)(runtime.CurrentIteration)/(float)MaxIter
                );
        }
    }
}
