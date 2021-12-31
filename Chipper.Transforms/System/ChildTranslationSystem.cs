using Unity.Entities;

namespace Chipper.Transforms
{
    public class ChildTranslationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var positions = GetComponentDataFromEntity<Position2D>();

            Entities
            .WithName("ChildTranslationSystem")
            .WithNativeDisableParallelForRestriction(positions)
            .ForEach((Entity entity, ref Parent2D parent) =>
            {
                if (positions.HasComponent(parent.Value))
                {
                    positions[entity] = new Position2D
                    {
                        Value = positions[parent.Value].Value + parent.Offset
                    };
                }
            })
            .ScheduleParallel();
        }
    }
}
