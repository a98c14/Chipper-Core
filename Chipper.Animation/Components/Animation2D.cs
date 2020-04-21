using Unity.Entities;
using Chipper.Rendering;

namespace Chipper.Animation
{
    [System.Serializable]
    public struct Animation2D
    {
        public bool  IsCreated  => Animation.IsCreated;
        public int   ID         => Animation.Value.ID;
        public int   Length     => Animation.Value.FrameCount;
        public int   Priority   => Animation.Value.Priority;
        public float Duration   => Animation.Value.Duration;
        public AnimationTransition TransitionType => Animation.Value.TransitionType;

        public ref SpriteID this[int frame] => ref Animation.Value.Sprites[frame];
        public bool IsOutOfBounds(int frame) => frame >= Length || frame < 0;

        public BlobAssetReference<AnimationBlob> Animation;
    }
}
