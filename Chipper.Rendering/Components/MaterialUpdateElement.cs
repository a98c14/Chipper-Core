using Unity.Entities;
using Unity.Mathematics;

namespace Chipper.Rendering
{
    public struct MaterialUpdateElement : IBufferElementData
    {
        public int                PropertyID;
        public float4             Value;
        public MaterialUpdateType Type;

        public MaterialUpdateElement(int id, float value) => 
            (PropertyID, Value, Type) = (id, value, MaterialUpdateType.Float);

        public MaterialUpdateElement(int id, float4 value) => 
            (PropertyID, Value, Type) = (id, value, MaterialUpdateType.Vector);

        public MaterialUpdateElement(int id, UnityEngine.Color value) => 
            (PropertyID, Value, Type) = (id, new float4(value.r, value.g, value.b, value.a), MaterialUpdateType.Color);

        public MaterialUpdateElement(int id, UnityEngine.Color32 value) =>
            (PropertyID, Value, Type) = (id, new float4(value.r, value.g, value.b, value.a), MaterialUpdateType.Color);
    }

    public enum MaterialUpdateType
    {
        Float,
        Vector,
        Color,
    }
}
