using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Orchestration
{
    //public class BossFightOrchestrator // Single use
    //{
    //    private readonly FightDefinition _definition;
    //    public BossFightOrchestrator(FightDefinition definition)
    //    {
    //        _definition = definition;
    //    }
    //    public async Task RunFightAsync(FightContext ctx)
    //    {
    //        await RunPhaseAsync(FightPhase.PreFightInit, ctx);
    //    }

    //    private async Task RunPhaseAsync(FightPhase phase, FightContext ctx)
    //    {
    //        PluginLog.Info($"Starting Boss fight phase {phase}");

    //        var tasks = _definition.Services.Select(service => service.OnPhaseAsync(phase, ctx));

    //        await Task.WhenAll(tasks);
    //    }
    //}
}
