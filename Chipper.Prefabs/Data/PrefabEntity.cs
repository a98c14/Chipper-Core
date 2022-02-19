using Chipper.Prefabs.Types;
using System.Collections.Generic;
using Unity.Entities;

namespace Chipper.Prefabs.Data
{
    public struct PrefabEntity
    {
        /// <summary>
        /// Id used by Hyperion
        /// </summary>
        public int Id;

        /// <summary>
        /// Name of the prefab
        /// </summary>
        public string Name;

        /// <summary>
        /// Converted entity
        /// </summary>
        public Entity Entity;
    }
}
