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
        public int MaterialId;
        public int SpriteId;

        public bool FlipX;
        public bool FlipY;

        public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new SpriteID
            {
                Value = conversionSystem.GetSpriteIndex(SpriteId),
            });

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
                MaterialID = MaterialId,
            });
        }
    }
}
