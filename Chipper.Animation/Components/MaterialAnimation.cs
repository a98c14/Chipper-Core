using Unity.Entities;
using Unity.Mathematics;
using Chipper.Rendering;

[System.Serializable]
[InternalBufferCapacity(3)]
public struct MaterialAnimation : IBufferElementData
{
    public float               Time;
    public float4              InitialValue;
    public float4              FinalValue;
    public float               Duration;

    // Constant
    public int                 PropertyID;
    public MaterialUpdateType  Type;
    public CurveType           Curve;
    public AnimationTransition Transition;
}
