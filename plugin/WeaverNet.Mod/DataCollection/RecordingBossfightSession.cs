using System.Collections.Generic;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Orchestration;
using WeaverNet.Core.Orchestration.Interfaces;
using WeaverNet.Core.Plugins;
using WeaverNet.Core.Plugins.FrameCapture;
using WeaverNet.Core.Plugins.Recording;
using WeaverNET.Infrastructure.Databases;

namespace WeaverNet.Mod.DataCollection
{
    internal class RecordingBossfightSessionFactory<TFrame> : IBossfightSessionPluginFactory
    {
        public string Id => _id;
        private readonly string _id;
        private readonly IFixedUpdataFrameSourceFactory<TFrame> _frameSourceFactory;
        private readonly IRecordingWriter _frameFileWriter;
        private readonly IRecordingRepository _recordingRepository;
        private readonly RecordingFrameTypeDefinition _frameTypeDefinition;
        private readonly string _outputPath;
        public RecordingBossfightSessionFactory(
            string Id,
            string outputPath,
            RecordingFrameTypeDefinition frameTypeDefinition,
            IFixedUpdataFrameSourceFactory<TFrame> frameSourceFactory, 
            IRecordingWriter frameFileWriter, 
            IRecordingRepository recordingRepository)
        {
            _id = Id;
            _outputPath = outputPath;
            _frameTypeDefinition = frameTypeDefinition;
            _frameSourceFactory = frameSourceFactory;
            _frameFileWriter = frameFileWriter;
            _recordingRepository = recordingRepository;
        }

        /// <summary>
        /// im 99% sure that if someone calls this during session something will break
        /// </summary>
        public IReadOnlyList<IBossfightSessionPlugin> CreatePlugins(BossfightSessionConfiguration config) 
        {
            return new IBossfightSessionPlugin[]
            {
                new FrameChannelPlugin<TFrame>(_frameSourceFactory,
                    new IFrameChannelPluginSink<TFrame>[]
                    {
                        new RecordingCoordinator<TFrame>(_outputPath, _frameTypeDefinition, _frameFileWriter, _recordingRepository)
                    })
            };
        }
    }
}
