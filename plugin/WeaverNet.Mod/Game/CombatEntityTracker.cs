using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Mod.Game
{
    internal class CombatEntityTracker : ICombatEntityTracker, ICombatEntityQuery
    {
        private Dictionary<HealthManager, EnemyInstance> _enemies = new Dictionary<HealthManager, EnemyInstance>();
        public IReadOnlyCollection<EnemyInstance> Enemies => _enemies.Values;
        public int EnemyCount => _enemies.Count;

        public event Action<EnemyInstance> OnEnemyRegistered;
        public event Action<EnemyInstance> OnEnemyUnregistered;
        public void RegisterEnemy(GameObject enemy)
        {
            if (!enemy.TryGetComponent<HealthManager>(out var healthManager))
            {
                PluginLog.Warning($"Tried to register enemy without HealthManager: {enemy.name}");
                return;
            }
            var instance = new EnemyInstance
            (
                enemy.name,
                enemy.GetInstanceID(),
                enemy,
                healthManager,
                enemy.GetComponent<Rigidbody2D>(),
                enemy.GetComponents<PlayMakerFSM>()
            );

            _enemies[healthManager] = instance;
            OnEnemyRegistered?.Invoke(instance);
        }

        public void UnregisterEnemy(GameObject enemy)
        {
            if (!enemy.TryGetComponent<HealthManager>(out var healthManager))
                return;

            if(_enemies.TryGetValue(healthManager, out var instance))
            {
                _enemies.Remove(healthManager);
                OnEnemyUnregistered?.Invoke(instance);
            }
        }
        public bool TryGetEnemy(HealthManager healthManager, out EnemyInstance enemy)
        {
            return _enemies.TryGetValue(healthManager, out enemy);
        }
        public bool TryGetEnemy(int id, out EnemyInstance enemy)
        {
            try
            {
                enemy = _enemies.Values.First(e => e.Id == id);
                return true;
            }
            catch
            {
                enemy = null;
                return false;
            }
        }
        public bool TryGetEnemy(Predicate<EnemyInstance> predicate, out EnemyInstance enemy)
        {
            try
            {
                foreach (var e in _enemies.Values)
                { 
                    if (predicate(e))
                    {
                        enemy = e;
                        return true;
                    }
                }

                enemy = null;
                return false;
            }
            catch
            {
                enemy = null;
                return false;
            }
        }
        public IEnumerable<EnemyInstance> GetEnemiesByName(string name)
        {
            return _enemies.Values.Where(e => e.Name == name);
        }
        public async Task<EnemyInstance> WaitForEnemyAsync(Predicate<EnemyInstance> predicate, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            //check if it is already present on scene:
            foreach (var enemy in _enemies.Values)
            {
                if (predicate(enemy))
                    return enemy;
            }

            var tcs = new TaskCompletionSource<EnemyInstance>(TaskCreationOptions.RunContinuationsAsynchronously);
            //await the enemy otherwise:
            void Handler(EnemyInstance instance)
            {
                if (predicate(instance))
                {
                    tcs.TrySetResult(instance);
                }
            }

            OnEnemyRegistered += Handler;
            using (ct.Register(() => tcs.TrySetCanceled(ct)))
            {
                try
                {
                    return await tcs.Task;
                }
                finally
                {
                    OnEnemyRegistered -= Handler;
                }
            }
        }
    }
}
