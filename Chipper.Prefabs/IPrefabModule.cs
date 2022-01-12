using Unity.Entities;

namespace Chipper.Prefabs
{
    public interface IPrefabModule
    {
        /// <summary>
        /// Converts the given json value to entity components
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem);
    }
}
