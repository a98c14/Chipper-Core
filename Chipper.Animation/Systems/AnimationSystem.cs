using Unity.Entities;
using Unity.Jobs;
using Chipper.Rendering;

namespace Chipper.Animation
{
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public partial class AnimationSystem : SystemBase
    {
        BeginInitializationEntityCommandBufferSystem m_CommandBufferSystem;

        protected override void OnCreate()
        {
            m_CommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

            Entities
                .WithName("AnimationSystem")
                .ForEach((Entity entity, int entityInQueryIndex, ref Animator2D animator, ref SpriteID sprite) =>
                {
                    animator.Clock += dt;
                    if (animator.Clock >= animator.Speed)
                    {
                        var animation = animator.Animation;
                        if (!animation.IsCreated)
                            return;

                        animator.Clock -= animator.Speed;
                        animator.Frame++;

                        if (animation.IsOutOfBounds(animator.Frame))
                            Transition(entityInQueryIndex, entity, ref animator, commandBuffer);

                        sprite = animation[animator.Frame];
                    }
                })
                .Schedule();

            m_CommandBufferSystem.AddJobHandleForProducer(Dependency);
        }

        static void Transition(int index, Entity entity, ref Animator2D animator, EntityCommandBuffer.ParallelWriter commandBuffer)
        {
            switch (animator.TransitionType)
            {
                case AnimationTransition.Loop:
                    animator.Frame = 0;
                    break;
                case AnimationTransition.Freeze:
                    animator.Frame = animator.FrameCount - 1;
                    break;
                case AnimationTransition.Destroy:
                    animator.Frame = animator.FrameCount - 1;
                    commandBuffer.DestroyEntity(index, entity);
                    break;
            }
        }
    }
}
