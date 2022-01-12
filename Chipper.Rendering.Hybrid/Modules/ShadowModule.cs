using Chipper.Prefabs;
using Unity.Entities;
using UnityEngine;

public class ShadowModule : IPrefabModule
{
    public float Size;
    public Vector2 Offset;
    public Vector2 Scale = new Vector2(1, 1);

    public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Shadow
        {
            Offset = Offset,
            Size = Size,
            Scale = Scale,
        });
    }
}
