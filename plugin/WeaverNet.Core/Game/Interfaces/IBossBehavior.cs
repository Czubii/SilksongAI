using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface IBossBehavior
    {
        void Respawn();
        void RespawnTemporary(TemporaryStateModifier modifier);
    }
}
