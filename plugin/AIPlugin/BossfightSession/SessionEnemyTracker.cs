using AIPlugin.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AIPlugin.BossfightSession
{
    public class SessionEnemyTracker
    {
        private BossMetadata _target;
        public void SetTargetEnemy(BossMetadata target)
        {
            _target = target;
        }
        public EnemyInstance GetTargetInstance()
        {
            return EnemyTracker.GetAllEnemies().FirstOrDefault(e => e.Name == _target.InternalName);
        }
        public List<EnemyInstance> GetNonTargetInstances()
        {
            List <EnemyInstance> all = EnemyTracker.GetAllEnemies().ToList();
            all.Remove(GetTargetInstance());
            return all;
        }
        public IEnumerator AwaitTargetOnScene(Action<bool> foundTarget, Func<bool> cancel = null) 
        {
            int i = 0;
            yield return new WaitUntil(() =>
            {
                if (cancel != null && cancel())
                {
                    foundTarget(false);
                    return true;
                }
                if (i >= SessionConfig.AwaitBossTimeoutFrames)
                {
                    AIPlugin.Log.LogWarning($"AwaitBossAndStartRecording(): Timeout hit when awaiting {_target.InternalName} " +
                        $"(timeout frames setting: {SessionConfig.AwaitBossTimeoutFrames} ");
                    foundTarget(false);
                    return true;
                }
                var boss = EnemyTracker
                    .GetAllEnemies()
                    .FirstOrDefault(e => e.Name == _target.InternalName);

                if (boss != null)
                {
                    foundTarget(true);
                    return true;
                }

                i++;
                return false;
            });
        }
    }
}
