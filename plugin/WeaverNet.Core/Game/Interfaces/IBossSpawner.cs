using WeaverNet.Core.Infrastructure;

namespace WeaverNet.Core.Game.Interfaces
{
    public interface IBossSpawner
    {
        void Respawn(BossRespawnFlags flags);
        void RespawnTemporary(BossRespawnFlags flags, TemporaryStateModifier modifier);
    }
}
