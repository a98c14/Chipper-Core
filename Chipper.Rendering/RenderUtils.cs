using Unity.Mathematics;

namespace Chipper.Rendering
{
    public struct RenderUtil
    {
        public static float3 GetRenderPosition(float3 pos)
            => new float3(pos.x, pos.y + pos.z * Constant.ZScale, 0);
    }
}

