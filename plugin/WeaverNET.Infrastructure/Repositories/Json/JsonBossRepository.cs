using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNET.Infrastructure.Data.Json
{
    public class JsonBossRepository: JsonReadOnlyRepository<BossData>, IBossRepository
    {
        public JsonBossRepository(string repositoryRoot) : base(repositoryRoot, boss => boss.Id)
        {
        }
    }
}
