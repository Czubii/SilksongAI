using System;
namespace WeaverNet.Core.Plugins
{
    public class RecordingFrame
    {
        public DateTime CreatedOn;
        public int FrameNumber { get; }
        public RecordingFrame() 
        { 
            CreatedOn = DateTime.Now;
           // FrameNumber = frameNumber; 
        }
    }
}
