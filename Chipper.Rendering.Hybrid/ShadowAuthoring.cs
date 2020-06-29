using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[AddComponentMenu("Game/Shadow Authoring")]
[RequiresEntityConversion]
public class ShadowAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float    Size;
    public Vector2  Offset;
    public Vector2  Scale = new Vector2(1,1);

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Shadow
        {
            Offset = Offset,
            Size = Size,
            Scale = Scale,
        });
    }
}