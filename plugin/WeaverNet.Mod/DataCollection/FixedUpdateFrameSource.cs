using System;
using System.Threading.Channels;
using UnityEngine;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Plugins;

namespace WeaverNet.Mod.DataCollection
{
    public class FixedUpdateFrameSource : MonoBehaviour, IFixedUpdateDataSource<RecordingFrame>
    {
        private ChannelWriter<RecordingFrame> _output = null;
        public bool IsRunning => _output != null;
        public void Attach(ChannelWriter<RecordingFrame> output)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("A channel is already attached");
            }
            _output = output;
        }

        public void Detach()
        {
            _output = null;
        }

        void FixedUpdate()
        {
            if (_output == null) return;

            _output.TryWrite(new RecordingFrame()); // TODO actuall collection
        }
    }
}
