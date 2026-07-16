using System.Threading.Tasks;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionPlugin
    {
        Task OnSessionStart();
        Task OnSessionEnd();
        Task OnFightStart(BossfightContext context);
        Task OnFightEnd(FightResult result);
    }
}
