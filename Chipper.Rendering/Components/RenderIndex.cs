using Unity.Entities;

public struct RenderIndex : ISystemStateComponentData
{
    public int Pool;
    public int Value;

    public static RenderIndex Null => new RenderIndex { Pool = -1, Value = -1 };
}
