using BepInEx.Configuration;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIPlugin.Utilities
{
    public class ConfigurationManagerAttributes
    {
        public bool? IsAdvanced = null;
        public Action<ConfigEntryBase> CustomDrawer = null;
    }
    public static class SessionConfig
    {
        public enum RecordingOutputTypes
        {
            JSON, // Only for debugging. Python wont be able to load those recording as of now
            MSGPACK
        }
        public static readonly int CaptureFrameDelta = 10;
        public static readonly RecordingOutputTypes RecordingOutputType = RecordingOutputTypes.MSGPACK;
        public static readonly int AwaitBossTimeoutFrames = 2000;
    }
    

}
