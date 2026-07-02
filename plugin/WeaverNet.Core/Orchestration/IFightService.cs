using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Orchestration
{
    public interface IFightService
    {
        Task OnPhaseAsync(FightPhase phase, FightContext context);
    }
}
