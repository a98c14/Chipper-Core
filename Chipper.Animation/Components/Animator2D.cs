using Unity.Entities;

namespace Chipper.Animation
{
    public struct  Animator2D : IComponentData
    {
        public int FrameCount                     => Animation.Length;
        public int CurrentPriority                => Animation.Priority;
        public AnimationTransition TransitionType => Animation.TransitionType;

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

        public void ClearIf(Animation2D anim)
        {
            if (Animation.ID == anim.ID)
                CanBeOverridden = true;
        }

        public void Clear() => CanBeOverridden = true;

        public bool IsPlaying(Animation2D anim) => Animation.ID == anim.ID;
    }
}
