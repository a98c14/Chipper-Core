using Unity.Entities;

namespace Chipper.Transforms
{
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    class DestroyPostConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((DestroyPostConversionAuthoring destroy) =>
            {
                var entity = GetPrimaryEntity(destroy);
                DstEntityManager.DestroyEntity(entity);
            });
        }
    }
}