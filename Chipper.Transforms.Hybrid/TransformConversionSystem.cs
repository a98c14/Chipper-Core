using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

namespace Chipper.Transforms
{
    class TransformConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Transform transform) =>
            {
                var entity = GetPrimaryEntity(transform);
                var hasParent = HasPrimaryEntity(transform.parent);
                var scale = hasParent ? transform.localScale : transform.lossyScale; 

                DstEntityManager.RemoveComponent<LocalToWorld>(entity);
                DstEntityManager.RemoveComponent<Translation>(entity);
                DstEntityManager.RemoveComponent<Rotation>(entity);

                if (hasParent)
                {
                    var parent = GetPrimaryEntity(transform.parent);
                    DstEntityManager.AddComponentData(entity, new Parent2D { Value = parent });
                    DstEntityManager.RemoveComponent<LocalToParent>(entity);
                    DstEntityManager.RemoveComponent<Parent>(entity);
                }

                DstEntityManager.AddComponentData(entity, new Position2D { Value = transform.position });
                DstEntityManager.AddComponentData(entity, new Rotation2D { Value = transform.eulerAngles.z });
                DstEntityManager.AddComponentData(entity, new Scale2D { Value = new float2(scale.x, scale.y)});            
            });
        }
    }
}