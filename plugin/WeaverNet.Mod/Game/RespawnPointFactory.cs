using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;

namespace WeaverNet.Mod.Game
{
    public class RespawnPointFactory : IRespawnPointFactory
    {
        public IRespawnPoint DefaultRespawnPoint(string name = null)
        {
            return new RespawnPoint("Tut_01", new Vector3(0, 0, 0), name);
        }

        public IRespawnPoint RespawnPoint(string scene, Vector3 position, string name = null)
        {
            return new RespawnPoint(scene, position, name);
        }
    }
}
