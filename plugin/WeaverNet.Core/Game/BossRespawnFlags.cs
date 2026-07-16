using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaverNet.Core.Game
{
    public class BossRespawnFlags
    {
        public Dictionary<string, bool> PlayerDataBools { get; }
        public List<SceneBool> SceneBools { get; }
        public List<SceneInt> SceneInts { get; }
        public BossRespawnFlags(Dictionary<string, bool> playerDataBools, List<SceneBool> sceneBools, List<SceneInt> sceneInts)
        {
            PlayerDataBools = playerDataBools;
            SceneBools = sceneBools;
            SceneInts = sceneInts;
        }
    }
}
