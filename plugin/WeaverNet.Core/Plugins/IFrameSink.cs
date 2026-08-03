using System.Threading.Tasks;
using WeaverNet.Core.Orchestration;
using System.Threading.Channels;

namespace WeaverNet.Core.Plugins
{
    public interface IFrameSink
    {
        Task OnSessionStartAsync();
        Task OnSessionEndAsync();
        Task OnFightStartAsync(FightContext context);
        Task OnFightEndAsync(FightResult result);
        ChannelWriter<FrameData> BeginStream();
    }
}
