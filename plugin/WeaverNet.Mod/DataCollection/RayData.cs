using UnityEngine;

namespace WeaverNet.Core.DataCollection
{
    public class RayData
    {
        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
        public float Distance { get; }
        public bool Hit { get; }
        public RaycastHit2D HitInfo { get; }
        public Vector3 Point => Hit ? (Vector3)HitInfo.point + new Vector3(0, 0, Origin.z) : Origin + (Direction * Distance);
        public Vector3 Normal => Hit ? (Vector3)HitInfo.normal : Vector3.zero;

        public RayData(Vector3 origin, Vector3 direction, float distance, bool hit, RaycastHit2D hitInfo)
        {
            Origin = origin;
            Direction = direction;
            Distance = distance;
            Hit = hit;
            HitInfo = hitInfo;
        }
    }
}