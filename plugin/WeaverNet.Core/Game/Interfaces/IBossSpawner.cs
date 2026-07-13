using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface IBossSpawner
    {
        void Respawn();
        void RespawnTemporary(TemporaryStateModifier modifier);
    }
}
