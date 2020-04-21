using UnityEngine;

namespace Chipper.Rendering
{
    public class ParticleConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ParticleSystem particleSystem) =>
            {
                var entity = GetPrimaryEntity(particleSystem);
                var gameObject = particleSystem.gameObject;
                if(ParticleConverter.GetComponent(gameObject, out var particleComponent))
                {
                    DstEntityManager.AddComponentData(entity, particleComponent);
                }
            });
        }
    }
}
