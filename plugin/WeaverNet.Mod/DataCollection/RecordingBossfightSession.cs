using System.Collections.Generic;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Core.Plugins;
using WeaverNET.Infrastructure.Databases;

namespace WeaverNet.Mod.DataCollection
{
    internal class RecordingBossfightSession: IBossfightSessionType
    {
        public string Type => "recording";
        private readonly IFixedUpdateDataSource<FrameData> _frameSource;
        private readonly IChannelFileWriter<FrameData> _frameFileWriter;
        private readonly string _outputPath;
        public RecordingBossfightSession(string outputPath, IFixedUpdateDataSource<FrameData> frameSource, IChannelFileWriter<FrameData> frameFileWriter)
        {
            _outputPath = outputPath;
            _frameSource = frameSource;
            _frameFileWriter = frameFileWriter;
        }
        public IReadOnlyList<IBossfightSessionPlugin> CreatePlugins(BossfightSessionConfiguration config)
        {
            return new IBossfightSessionPlugin[]
            {
                new FramePipelinePlugin(_frameSource,
                    new IFrameSink[]
                    {
                        new RecordingManager(_outputPath, _frameFileWriter)
                    })
            };
        }
    }
}
