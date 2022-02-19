namespace Chipper.Prefabs.Types
{
    public enum EditorType
    {
        Object = 0,
        Range,
        Color,
        Animation,
        Sprite,
        Percentage,
        Vec2,
        Vec3,
        Vec4,
        Nested, // Deprecated
        Bool,
        Number,
        Text,
        TextArea,
        Material,
        ParticleSystem,
        TrailSystem,
        Audio,
        Prefab,
        ItemPool,
        StatusEffect,
        SpawnPrefab, // Refer to Spawnable.cs
        SkillSpawnPrefab, // Refer to Spawnable.cs
        MaterialAnimation,
        Enum,
        DamageType,
    }
}
