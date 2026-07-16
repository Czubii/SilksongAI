using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Game
{
    public class SceneBool
    {
        public string SceneName { get; }
        public string ID { get; }
        public bool Value { get; }
        public SceneBool(string sceneName, string id, bool value)
        {
            SceneName = sceneName;
            ID = id;
            Value = value;
        }
    }
}
