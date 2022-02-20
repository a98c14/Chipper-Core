using Chipper.Animation;
using Chipper.Prefabs.Data;
using Chipper.Prefabs.Types;
using System.Collections.Generic;
using Unity.Entities;

namespace Chipper.Prefabs
{
    public interface IPrefabConversionSystem
    {
        List<PrefabEntity> PrefabEntities { get; }
        bool TryGetEntity(int prefabId, out Entity prefabEntity);
        bool TryGetEntity(string prefabName, out Entity prefabEntity);
        bool TryGetPrefabEntity(int prefabId, out PrefabEntity prefabEntity);
        bool TryGetPrefabEntity(string prefabName, out PrefabEntity prefabEntity);
        UnityEngine.Sprite GetSprite(int spriteId);
        int GetSpriteIndex(int spriteId);

        int GetUnityRenderLayerId(RenderLayer layer);
        int GetUnitySortingLayerId(RenderSortLayer layer);

        Animation2D GetAnimation(int id);
        MaterialAnimation GetMaterialAnimation(int id);
    }
}
