using Unity.Entities;
using Chipper.Rendering;

namespace Chipper.Animation 
{ 
    public struct AnimationBlob
    {
        public int FrameCount => Sprites.Length;
        public float Duration => FrameCount * Constant.DefaultAnimationSpeed;

        public int ID;
        public int Priority;
        public AnimationTransition TransitionType;
        public BlobArray<SpriteID> Sprites;
    }
}
