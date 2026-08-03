using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionAssembler
    {
        BossfightSession Assemble(string type, BossfightSessionConfiguration configuration);
    }
}
