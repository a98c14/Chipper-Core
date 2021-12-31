using Unity.Entities;
using UnityEngine;

namespace Chipper.Rendering
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Rendering/Material Property Authoring")]
    public class MaterialPropertyAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<MaterialUpdateElement>(entity);
        }
    }
}
