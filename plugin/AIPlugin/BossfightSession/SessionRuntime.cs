using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.BossfightSession
{
    public sealed class SessionRuntime: IDisposable
    {
        public int RemainingFights { get; private set; }
        public int AttemptIndex { get; private set; }
        public int TotalFights { get; private set; }
        public CustomRespawnPoint RespawnPoint { get; private set; }    
        public AttemptResult LastAttempt { get; set; }
        public SessionRuntime(int totalFights, CustomRespawnPoint respawnPoint, AttemptResult lastAttempt) 
        {
            TotalFights = totalFights;
            RemainingFights = totalFights;
            AttemptIndex = 0;
            RespawnPoint = respawnPoint;
            LastAttempt = lastAttempt;
        }
        public static SessionRuntime Start(SessionContext ctx) 
        {
            var spawnpoint = new CustomRespawnPoint(
                "BossFightSessionRespawn" + ctx.Boss.InternalName,
                ctx.Boss.ArenaSceneName,
                ctx.Boss.ArenaPosition);

            return new SessionRuntime(ctx.TotalFights, spawnpoint, AttemptResult.FirstAttempt);
        }

        public void StartAttempt()
        {
            AttemptIndex++;
            RemainingFights--;
        }

        public void Dispose()
        {
            RespawnPoint?.Dispose();
            RespawnPoint = null;
            CustomRespawnPoint.ResetTemporary();
        }
    }
}
