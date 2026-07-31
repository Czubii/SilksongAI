using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverNet.Core.DataCollection;

namespace WeaverNet.Mod.DataCollection
{
    public class HeroSurroundingsScanner : IRaycastScanner
    {
        private const int RaysPerQuadrant = 4;
        private const int TotalRays = RaysPerQuadrant * 4;
        private const float rayStepRad = Mathf.PI / (2 * RaysPerQuadrant);
        private const float ScanDistance = 10.0f;
        private Transform GetHeroTransform()
        {
            return HeroController.instance?.transform;
        }
        public bool TryScanTerrain(out IReadOnlyList<RayData> rays)
        {
            return TryScan((int)RaycastTargetLayers.DefaultTerrain, out rays);
        }
        public bool TryScan(int mask, out IReadOnlyList<RayData> rays)
        {
            rays = Array.Empty<RayData>();

            var transform = GetHeroTransform();
            if (transform == null) return false;

            var result = new List<RayData>(TotalRays);
            Vector3 origin = transform.position;

            for (int i = 0; i < TotalRays; i++)
            {
                var angleRad = rayStepRad * i;
                Vector3 direction = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0.0f);

                result.Add(CastRay(origin, direction, ScanDistance, mask));
            }

            rays = result;
            return true;
        }
       
        private RayData CastRay(Vector3 origin, Vector3 direction, float distance, int mask)
        {
            Vector3 normDir = direction.normalized;

            // Cast in 2D using Vector2 slice of origin and direction
            RaycastHit2D hit = Physics2D.Raycast(
                (Vector2)origin,
                (Vector2)normDir,
                distance,
                mask
            );

            bool didHit = hit.collider != null;

            return new RayData(
                origin,
                normDir,
                distance,
                didHit,
                hit
            );
        }
    }
}