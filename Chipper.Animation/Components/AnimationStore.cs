using Unity.Entities;

namespace Chipper.Animation
{
    [System.Serializable]
    public struct AnimationStore : IBufferElementData
    {
        public Animation2D Value;
    }
}
