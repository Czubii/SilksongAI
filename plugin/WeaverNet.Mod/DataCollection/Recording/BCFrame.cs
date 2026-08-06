using WeaverNet.Core.Plugins.Recording;
using WeaverNet.Mod.DataCollection.Recording;
namespace WeaverNet.Core.Plugins
{
    public class BCFrame
    {
        public const int FormatVersion = 1;
        public FrameObservation Observation { get; }
        public FrameAction Action { get; }
        public BCFrame(FrameObservation observation, FrameAction action)
        {
            Observation = observation;
            Action = action;
        }
    }
}
