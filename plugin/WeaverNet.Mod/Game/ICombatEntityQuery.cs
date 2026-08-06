using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WeaverNet.Mod.Game
{
    public interface ICombatEntityQuery
    {
        IReadOnlyCollection<EnemyInstance> Enemies { get; }
        int EnemyCount { get; }
        bool TryGetEnemy(HealthManager healthManager, out EnemyInstance enemy);
        bool TryGetEnemy(int id, out EnemyInstance enemy);
        bool TryGetEnemy(Predicate<EnemyInstance> predicate, out EnemyInstance enemy);
        IEnumerable<EnemyInstance> GetEnemiesByName(string name);
        Task<EnemyInstance> WaitForEnemyAsync(Predicate<EnemyInstance> predicate, CancellationToken ct);
    }
}
