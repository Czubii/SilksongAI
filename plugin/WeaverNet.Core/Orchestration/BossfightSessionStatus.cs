using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration.Interfaces;

namespace WeaverNet.Core.Orchestration
{
    public class BossfightSessionStatus : IBossfightSessionStatus, IBossfightSessionStatusWriter
    {
        public bool IsRunning { get; private set; }
        public BossData CurrentBoss { get; private set; }
        public BossfightSessionProgress Progress { get; private set; }

        public event Action StatusChanged;
        public void Start(BossfightSession session)
        {
            IsRunning = true;
            CurrentBoss = session.Boss;
            Notify();
        }

        public void UpdateProgress(BossfightSessionRuntime runtime)
        {
            Progress = runtime.Session.Boundary.GetSessionProgress(runtime);
            Notify();
        }

        public void Stop()
        {
            IsRunning = false;
            CurrentBoss = null;
            Progress = null;
            Notify();
        }

        private void Notify()
        {
            try
            {
                StatusChanged?.Invoke();
            }
            catch(Exception ex) 
            {
                PluginLog.Error(ex.ToString());
            }
        }
    }
}
