using System.Collections.Generic;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Core.Plugins;
using WeaverNET.Infrastructure.Databases;

namespace WeaverNet.Mod.DataCollection
{
    internal class RecordingBossfightSession: IBossfightSessionType
    {
        public string Type => "recording";
        private readonly IFixedUpdateDataSource<RecordingFrame> _frameSource;
        private readonly IChannelFileWriter<RecordingFrame> _frameFileWriter;
        private readonly IRecordingRepository _recordingRepository;
        private readonly string _outputPath;
        public RecordingBossfightSession(
            string outputPath, 
            IFixedUpdateDataSource<RecordingFrame> frameSource, 
            IChannelFileWriter<RecordingFrame> frameFileWriter, 
            IRecordingRepository recordingRepository)
        {
            _outputPath = outputPath;
            _frameSource = frameSource;
            _frameFileWriter = frameFileWriter;
            _recordingRepository = recordingRepository;
        }
        public IReadOnlyList<IBossfightSessionPlugin> CreatePlugins(BossfightSessionConfiguration config)
        {
            return new IBossfightSessionPlugin[]
            {
                new FrameChannelPlugin<RecordingFrame>(_frameSource,
                    new IFrameChannelPluginSink<RecordingFrame>[]
                    {
                        new RecordingCoordinator(_outputPath, _frameFileWriter, _recordingRepository)
                    })
            };
        }
    }
}
