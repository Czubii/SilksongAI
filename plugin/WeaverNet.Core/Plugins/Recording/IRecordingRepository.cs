using System;
using WeaverNet.Core.Infrastructure.Interfaces;
using WeaverNet.Core.Plugins.Recording;

namespace WeaverNet.Core.Infrastructure
{
    public interface IRecordingRepository: IRepository<BossfightRecording, Guid>
    {
    }
}
