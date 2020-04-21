using Unity.Entities;
using Unity.Mathematics;

public struct Shadow : IComponentData
{
    public float2 Scale;
    public float2 Offset;
}
