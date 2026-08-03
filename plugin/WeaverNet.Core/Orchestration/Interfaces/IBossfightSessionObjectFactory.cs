using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverNet.Core.Game.Interfaces;
using WeaverNet.Core.Infrastructure;
using WeaverNet.Core.Plugins;

namespace WeaverNet.Core.Orchestration.Interfaces
{
    public interface IBossfightSessionObjectFactory
    {
        IRespawnPoint CreateRespawnPoint(string scene, Vector3 position, string name);
        IRespawnPoint CreateDefaultRespawnPoint();
    }
}
