using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface ICombatEntityTracker
    {
        void RegisterEnemy(GameObject enemy);
        void UnregisterEnemy(GameObject enemy);
    }
}