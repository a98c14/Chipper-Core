using Unity.Entities;

namespace Chipper.Rendering
{
    public struct SpriteID : IComponentData
    {
        public int Value;

        public SpriteID(int id) => Value = id;
    }
}
