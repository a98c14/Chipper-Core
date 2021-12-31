using Unity.Entities;
using UnityEngine;

namespace Chipper.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Animation/Sprite Animator Authoring")]
    public class SpriteAnimatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public SpriteAnimationObject Animation;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Animator2D
            {
                Speed = Constant.DefaultAnimationSpeed,
                Animation = Animation?.Component ?? default,
            });
        }
    }
}
