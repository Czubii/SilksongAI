using System.Collections.Generic;
using UnityEngine;

namespace WeaverNet.Mod.Game
{
    public static class PresetLayerMasks
    {
        public static readonly int None = 0;
        public static readonly int Everything = ~0;

        public static readonly int Obstacles = (int)(
            HitboxLayers.Terrain |
            HitboxLayers.Bouncer
        );

        public static readonly int Hazards = (int)(
            HitboxLayers.Attack |
            HitboxLayers.Enemies | // TODO take a look at this, as some enemies that are here dont do any damage
            HitboxLayers.EnemyAttack
        );
        public static readonly IReadOnlyDictionary<string, int> AllPresets = new Dictionary<string, int>
        {
            { "Everything", Everything },
            { "None", None },
            { "Obstacles", Obstacles },
            { "Hazards", Hazards }
        };

        public static int GetMask(params string[] layerNames)
        {
            return LayerMask.GetMask(layerNames);
        }
    }
}