using UnityEngine;

namespace Chipper.Rendering
{
    public class TrailRendererConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((TrailRenderer trailRenderer) =>
            {
                var entity = GetPrimaryEntity(trailRenderer);
                var gameObject = trailRenderer.gameObject;
                if (TrailConverter.GetComponent(gameObject, out var trailComponent))
                {
                    DstEntityManager.AddComponentData(entity, trailComponent);
                }
            });
        }
    }
}
