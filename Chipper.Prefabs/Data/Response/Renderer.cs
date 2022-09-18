using Chipper.Prefabs.Types;

namespace Chipper.Prefabs.Data.Response
{
    public class PrefabRenderer 
    {
        public int PrefabId { get; set; }
        public bool IsVisible { get ; set; }
        public int RenderLayer { get; set; }
        public int RenderSortLayer { get; set; }
        public string Color { get; set; }
        public int? MaterialAssetId { get; set; }
        public int? SpriteAssetId { get; set; }
        public int? AnimationAssetId { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
    }
}