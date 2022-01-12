using Chipper.Prefabs;
using Unity.Entities;

namespace Chipper.Rendering
{
    public class MaterialPropertyModule : IPrefabModule
    {
        public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem)
        {
            dstManager.AddBuffer<MaterialUpdateElement>(entity);
        }
    }
}
