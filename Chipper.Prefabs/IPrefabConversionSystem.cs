using Chipper.Animation;
using Chipper.Prefabs.Types;
using Unity.Entities;
using UnityEngine;

namespace Chipper.Prefabs
{
    public interface IPrefabConversionSystem
    {
        Entity GetPrefabEntity(int prefabId);
        Sprite GetSprite(int spriteId);
        
        int GetSpriteId(Sprite sprite);
        int GetMaterialId(Material material);

        int GetUnityRenderLayerId(RenderLayer layer);
        int GetUnitySortingLayerId(RenderSortLayer layer);

        Animation2D GetAnimation(int id);
        MaterialAnimation GetMaterialAnimation(int id);
        
    }
}
