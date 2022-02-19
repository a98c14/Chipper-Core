namespace Chipper.Prefabs.Data.Response
{
    // "transform\":{\"position\":{\"x\":2,\"y\":0,\"z\":0},\"scale\":{\"x\":1,\"y\":1},\"rotation\":0}"
    public class Transform
    {
        public Vec3 Position { get; set; }
        public Vec2 Scale    { get; set; }
        public float Rotation  { get; set; }
    }
}