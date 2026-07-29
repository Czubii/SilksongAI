using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface IRespawnPointFactory
    {
        IRespawnPoint RespawnPoint(string scene, Vector3 position, string name = null);
        IRespawnPoint DefaultRespawnPoint(string name = null);
    }
}
