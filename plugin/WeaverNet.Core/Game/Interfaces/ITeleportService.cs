using System.Threading.Tasks;
using UnityEngine;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface ITeleportService
    {
        void Teleport(string targetScene, Vector3 targetPos, bool requireSceneReload);
        void TeleportToBench();
        Task TeleportAsync(string targetScene, Vector3 targetPos, bool requireSceneReload);
        Task TeleportToBenchAsync();
        bool CanTeleport();
        bool TeleportInProgress();
    }
}
