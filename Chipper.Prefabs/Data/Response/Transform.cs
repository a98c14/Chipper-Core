using Newtonsoft.Json;

namespace Chipper.Prefabs.Data.Response
{
    // "transform\":{\"position\":{\"x\":2,\"y\":0,\"z\":0},\"scale\":{\"x\":1,\"y\":1},\"rotation\":0}"
    public class PrefabTransform
    {
        [JsonProperty("prefabId")]
        public int PrefabId  { get; set; }

        [JsonProperty("position")]
        public Vec3 Position { get; set; }

        [JsonProperty("scale")]
        public Vec3 Scale    { get; set; }

        [JsonProperty("rotation")]
        public Vec3 Rotation { get; set; }
    }
}