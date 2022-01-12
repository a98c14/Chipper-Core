using Chipper.Prefabs;
using Chipper.Prefabs.Types;
using Unity.Entities;
using UnityEngine;

namespace Chipper.Rendering
{
    public class RenderingModule : IPrefabModule
    {
        public RenderLayer Layer;
        public RenderSortLayer SortLayer;
        public Color Color;
        public Material Material;
        public Sprite Sprite;

        public bool FlipX;
        public bool FlipY;

        public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem)
        {
            var material = conversionSystem.GetMaterialId(Material);
            var sprite = new SpriteID(conversionSystem.GetSpriteId(Sprite));
            dstManager.AddComponentData(entity, sprite);

            dstManager.AddComponentData(entity, new RenderInfo
            {
                Color = Color,
                IsDefaultDirectionRight = !FlipX,
                Layer = conversionSystem.GetUnityRenderLayerId(Layer),
                SortingLayer = conversionSystem.GetUnitySortingLayerId(SortLayer),
            });

            dstManager.AddComponentData(entity, new RenderFlipState
            {
                flipX = FlipX,
                flipY = FlipY,
            });

            dstManager.AddSharedComponentData(entity, new MaterialInfo
            {
                MaterialID = material,
            });
        }

    }

}
