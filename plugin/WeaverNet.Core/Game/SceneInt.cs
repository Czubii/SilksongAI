using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Game
{
    public class SceneInt
    {
        public string SceneName { get; }
        public string ID { get; }
        public int Value { get; }
        public int Mutator { get; }

        public SceneInt(string sceneName, string id, int value, int mutator)
        {
            SceneName = sceneName;
            ID = id;
            Value = value;
            Mutator = mutator;
        }
    }
}
