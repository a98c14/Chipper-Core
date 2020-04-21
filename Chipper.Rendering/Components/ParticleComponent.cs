using Unity.Entities;

public enum ParticleDestroyMethod
{
    ByGameObject,
    ByEntity,
}

public struct ParticleComponent : IComponentData
{
    public int Id;
    public ParticleDestroyMethod DestroyMethod;
}
