using Unity.Entities;

namespace Chipper.Animation
{
    public struct  Animator2D : IComponentData
    {
        public bool IsNull                        => !Animation.IsCreated;
        public int FrameCount                     => Animation.IsCreated ? Animation.Length : 0;
        public int CurrentPriority                => Animation.IsCreated ? Animation.Priority : 0;
        public AnimationTransition TransitionType => Animation.IsCreated ? Animation.TransitionType : AnimationTransition.Loop;

        // If True, lower priority animations can override the current animation
        public bool  CanBeOverridden; 

        public int   Frame;
        public float Speed;
        public float Clock;

        public Animation2D Animation;

        public Animator2D(Animation2D anim)
        {
            Frame = 0;
            Clock = 0;
            CanBeOverridden = false;
            Speed = Constant.DefaultAnimationSpeed;
            Animation = anim;
        }

        public void Play(Animation2D anim, int frame = 0)
        {
            Frame = frame;
            Animation = anim;
            CanBeOverridden = false;
        }

        public void PlayIfNotSame(Animation2D anim, int frame = 0)
        {
            if (Animation.IsCreated && anim.ID == Animation.ID)
                return;

            Frame = frame;
            Animation = anim;
            CanBeOverridden = false;
        }

        public void ClearIf(Animation2D anim)
        {
            if (Animation.IsCreated && Animation.ID == anim.ID)
                CanBeOverridden = true;
        }

        public void Clear() => CanBeOverridden = true;

        public bool IsPlaying(Animation2D anim) => Animation.IsCreated && Animation.ID == anim.ID;
    }
}
