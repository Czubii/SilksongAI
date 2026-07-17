using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Game;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNET.Infrastructure.Data
{
    public class JsonLoadoutRepository: JsonRepository<Loadout>, ILoadoutRepository
    {
        public JsonLoadoutRepository(string repositoryRoot) : base(repositoryRoot, loadout => loadout.Name) { }
    }
}
