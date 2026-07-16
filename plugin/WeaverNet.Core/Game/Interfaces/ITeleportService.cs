using UnityEngine;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface ITeleportService
    {
        void Teleport(string targetSceneName, Vector3 targetPos, bool requireSceneReload);
        void TeleportToBench();
        bool CanTeleport();
    }
}
