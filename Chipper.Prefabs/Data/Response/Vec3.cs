using Newtonsoft.Json;
using Unity.Mathematics;

namespace Chipper.Prefabs.Data.Response
{
    public class Vec3
    {
        [JsonProperty("x")]
        public float X { get; set; }

        [JsonProperty("y")]
        public float Y { get; set; }

        [JsonProperty("z")]
        public float Z { get; set; }

        public float3 Float3 => new(X, Y, Z);
    }
}