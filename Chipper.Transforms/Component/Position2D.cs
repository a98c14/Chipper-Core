using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Transforms
{
    public struct Position2D : IComponentData
    {
        public float3 Value;

        public Position2D(float3 v) => Value = v;
    }
}
