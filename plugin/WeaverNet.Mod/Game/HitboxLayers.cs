using System;

namespace WeaverNet.Mod.Game
{
    [Flags]
    public enum HitboxLayers : uint
    {
        None = 0,
        Default = 1u << 0,
        TransparentFX = 1u << 1,
        IgnoreRaycast = 1u << 2,
        Water = 1u << 4,
        UI = 1u << 5,
        CurrencySelfCollide = 1u << 7,
        Terrain = 1u << 8,
        Player = 1u << 9,
        SelfCollide = 1u << 10,
        Enemies = 1u << 11,
        Projectiles = 1u << 12,
        HeroDetector = 1u << 13,
        TerrainDetector = 1u << 14,
        EnemyDetector = 1u << 15,
        Tinker = 1u << 16,
        Attack = 1u << 17,
        Particle = 1u << 18,
        InteractiveObject = 1u << 19,
        HeroBox = 1u << 20,
        Grass = 1u << 21,
        EnemyAttack = 1u << 22,
        WaterSurface = 1u << 23,
        Bouncer = 1u << 24,
        SoftTerrain = 1u << 25,
        Corpse = 1u << 26,
        PhysicalPusher = 1u << 27,
        HeroOnly = 1u << 28,
        ActiveRegion = 1u << 29,
        PhysicalPushReact = 1u << 30,
        AttackDetector = 1u << 31,

        Everything = 0xFFFFFFFF
    }
}
