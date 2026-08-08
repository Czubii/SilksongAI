using System;
using System.Collections.Generic;
using WeaverNet.Core.Game;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Core.Plugins.Recording;

namespace WeaverNet.Core.Infrastructure
{
    public interface IRecordingRepositoryQuery
    {
        IReadOnlyList<string> GetDistinctBossIds();
        IReadOnlyList<Loadout> GetDistinctLoadouts();
        int CountRecordings(string bossId = null, Loadout loadout = null);
    }
    public interface IRecordingRepository: IFileReferenceRepository<BossfightRecordingMetadata, Guid>, IRecordingRepositoryQuery
    { 
    }
}
