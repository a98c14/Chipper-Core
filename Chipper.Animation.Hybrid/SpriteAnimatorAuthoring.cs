using Unity.Entities;
using UnityEngine;

namespace Chipper.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequiresEntityConversion, DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Animation/Sprite Animator Authoring")]
    public class SpriteAnimatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public SpriteAnimationObject Animation;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            Animation2D comp;

            if (Animation != null)
                comp = Animation.Component;
            else
                comp = new Animation2D();

            dstManager.AddComponentData(entity, new Animator2D
            {
                Speed = Constant.DefaultAnimationSpeed,
                Animation = comp,
            });
        }
    }
}
