using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Transforms
{
    public struct Parent2D : IComponentData
    {
        public Entity Value;
        public float3 Offset;
    }
}
