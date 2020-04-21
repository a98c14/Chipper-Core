using Unity.Mathematics;

public static class AnimationUtil
{
    public static float EaseOutElastic(float t, float p = 1)
    {
        return math.pow(2, -10 * t) * math.sin((t - p / 4) * (2 * math.PI) / p) + 1;
    }

    public static float EaseInQuad(float t)
    {
        return math.pow(t, 2);
    }

    public static float EaseOutQuad(float t)
    {
        return -math.pow(t-1, 2) + 1;
    }

    public static float EaseInCubic(float t)
    {
        return math.pow(t, 3);
    }

    public static float EaseOutCubic(float t)
    {
        return math.pow(t-1, 3) + 1;
    }
    public static float EaseInQuart(float t)
    {
        return math.pow(t, 4);
    }

    public static float EaseOutQuart(float t)
    {
        return -math.pow(t-1, 4) + 1;
    }

    public static float EaseInQuint(float t)
    {
        return math.pow(t, 5);
    }

    public static float EaseOutQuint(float t)
    {
        return math.pow(t-1, 5) + 1;
    }

    public static float Linear(float t)
    {
        return t;
    }

    public static float ApplyCurve(CurveType type, float time, float duration)
    {
        var t = time / duration;
        t = math.clamp(t, 0, 1);
        switch (type)
        {
            case CurveType.EaseOutElastic:
                t = EaseOutElastic(t, 1);
                break;
            case CurveType.EaseInQuad:
                t = EaseInQuad(t);
                break;
            case CurveType.EaseOutQuad:
                t = EaseOutQuad(t);
                break;
            case CurveType.EaseInCubic:
                t = EaseInCubic(t);
                break;
            case CurveType.EaseOutCubic:
                t = EaseOutCubic(t);
                break;
            case CurveType.EaseInQuart:
                t = EaseInQuart(t);
                break;
            case CurveType.EaseOutQuart:
                t = EaseOutQuart(t);
                break;
            case CurveType.EaseInQuint:
                t = EaseInQuint(t);
                break;
            case CurveType.EaseOutQuint:
                t = EaseOutQuint(t);
                break;
            case CurveType.Linear:
                t = Linear(t);
                break;
        }
        return t;
    }

    public static float ApplyCurve(CurveType type, float time, float duration, float minValue, float maxValue)
    {
        var t = ApplyCurve(type, time, duration);
        return math.lerp(minValue, maxValue, t);
    }

    public static float2 ApplyCurve(CurveType type, float time, float duration, float2 minValue, float2 maxValue)
    {
        var t = ApplyCurve(type, time, duration);
        return math.lerp(minValue, maxValue, t);
    }

    public static float3 ApplyCurve(CurveType type, float time, float duration, float3 minValue, float3 maxValue)
    {
        var t = ApplyCurve(type, time, duration);
        return math.lerp(minValue, maxValue, t);
    }

    public static float4 ApplyCurve(CurveType type, float time, float duration, float4 minValue, float4 maxValue)
    {
        var t = ApplyCurve(type, time, duration);
        return math.lerp(minValue, maxValue, t);
    }
}
