using System;
namespace WeaverNet.Core.Plugins
{
    public class FrameData
    {
        public DateTime CreatedOn;
        public int FrameNumber { get; }
        public FrameData() 
        { 
            CreatedOn = DateTime.Now;
           // FrameNumber = frameNumber; 
        }
    }
}
