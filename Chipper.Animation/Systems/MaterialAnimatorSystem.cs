using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;

namespace Chipper.Rendering
{
    [UpdateInGroup(typeof(AnimationSystemGroup))]
    public class MaterialAnimatorSystem : JobComponentSystem
    {
        [BurstCompile]
        [RequireComponentTag(typeof(MaterialUpdateElement), typeof(MaterialAnimation))]
        struct Animate : IJobForEachWithEntity<Translation>
        {
            [ReadOnly] public float Dt;
            [ReadOnly] public EntityCommandBuffer.Concurrent  CommandBuffer;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<MaterialUpdateElement> MaterialUpdateBuffers;

            [NativeDisableParallelForRestriction]
            public BufferFromEntity<MaterialAnimation> MaterialAnimationBuffers;

            void Transition(Entity entity, int jobIndex, int materialIndex, ref MaterialAnimation animation, NativeList<int> removeList)
            {
                if(animation.Time >= animation.Duration)
                {
                    switch (animation.Transition)
                    {
                        case AnimationTransition.Loop:
                            animation.Time -= animation.Duration;
                            break;
                        case AnimationTransition.Freeze:
                            animation.Time = animation.Duration;
                            removeList.Add(materialIndex);
                            break;
                        case AnimationTransition.Destroy:
                            animation.Time = animation.Duration;
                            CommandBuffer.DestroyEntity(jobIndex, entity);
                            break;
                    }
                }
            }

            MaterialUpdateElement Evaluate(MaterialAnimation animation)
            {
                return new MaterialUpdateElement
                {
                    PropertyID = animation.PropertyID,
                    Type = animation.Type,
                    Value = AnimationUtil.ApplyCurve(animation.Curve, animation.Time, animation.Duration, animation.InitialValue, animation.FinalValue),
                };
            }

            public void Execute(Entity entity, int index,[ReadOnly] ref Translation translation)
            {
                var materialBuffer = MaterialUpdateBuffers[entity];
                var animationBuffer = MaterialAnimationBuffers[entity];
                var animations = animationBuffer.AsNativeArray();
                var removeList = new NativeList<int>(3, Allocator.Temp);
            
                for(int i = 0; i < animations.Length; i++)
                {
                    var animation = animations[i];
                    materialBuffer.Add(Evaluate(animation));
                    animation.Time += Dt;        
                    Transition(entity, index, i, ref animation, removeList);
                    animationBuffer[i] = animation;
                }

                for(int i = 0; i < removeList.Length; i++)
                {
                    animationBuffer.RemoveAt(removeList[i]);
                }
                removeList.Dispose();
            }
        }

        BeginInitializationEntityCommandBufferSystem m_CommandBufferSystem;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new Animate()
            {
                Dt = UnityEngine.Time.deltaTime,
                MaterialAnimationBuffers = GetBufferFromEntity<MaterialAnimation>(false),
                MaterialUpdateBuffers = GetBufferFromEntity<MaterialUpdateElement>(false),
                CommandBuffer = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
            m_CommandBufferSystem.AddJobHandleForProducer(job);
            return job;
        }

        protected override void OnCreate()
        {
            m_CommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        }
    }
}
