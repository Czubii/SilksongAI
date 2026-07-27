using System;
using System.Collections.Generic;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionRuntime
    {
        public BossfightSession Session { get; }
        public int CurrentIteration { get; private set; }
        public List<FightResult> ResultHistory { get; private set; }
        public BossfightSessionRuntime(BossfightSession session)
        {
            Session = session;
            CurrentIteration = 0;
            ResultHistory = new List<FightResult>();
        }
        public FightContext IterationStarted()
        {
            return new FightContext(
                Session.Boss,
                Session.Loadout,
                DateTime.Now,
                Guid.NewGuid(),
                CurrentIteration
            );
        }
        public void IterationFinished(FightResult result)
        {
            CurrentIteration++;
            ResultHistory.Add(result);
        }
        public bool FirstIteration => CurrentIteration == 0;
        /// <returns>result of previous iteration (null if its the first iteration)</returns>
        public FightResult? PreviousIterationResult()
        {
            if (ResultHistory.Count == 0) return null;
            return ResultHistory[ResultHistory.Count - 1];
        }
    }
}
