using Chipper.Prefabs;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Chipper.Transforms
{
    public class TransformModule : IPrefabModule
    {
        public Vector3 Position;
        public float Rotation;
        public Vector2 Scale;

        public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem)
        {
            // Since default unity transform systems aren't used
            // default components are removed to stop systems from running.
            dstManager.RemoveComponent<LocalToWorld>(entity);
            dstManager.RemoveComponent<Translation>(entity);
            dstManager.RemoveComponent<Rotation>(entity);

            dstManager.AddComponentData(entity, new Position2D { Value = Position });
            dstManager.AddComponentData(entity, new Rotation2D { Value = Rotation });
            dstManager.AddComponentData(entity, new Scale2D { Value = new float2(Scale.x, Scale.y) });
        }
    }
}
