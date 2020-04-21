using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Transforms
{
    public struct Scale2D : IComponentData
    {
        public static Scale2D One => new Scale2D(1);

        public float2 Value;

        public Scale2D(float v) => Value = v;
        public Scale2D(float x, float y) => Value = new float2(x, y);
        public Scale2D(float2 v) => Value = v;
    }
}
