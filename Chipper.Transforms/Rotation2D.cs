using Unity.Entities;

namespace Chipper.Transforms
{
    public struct Rotation2D : IComponentData
    {
        public float Value;

        public Rotation2D(float v) => Value = v;
    }
}
