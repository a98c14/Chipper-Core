using Chipper.Prefabs;
using Chipper.Prefabs.Attributes;
using Chipper.Prefabs.Types;
using Unity.Entities;
using UnityEngine;

namespace Chipper.Animation
{
    [RequireComponent(typeof(SpriteRenderer))]
    [DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Animation/Sprite Animator Authoring")]
    public class SpriteAnimatorAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [EditorType(EditorType.Animation)]
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

    public class SpriteAnimatorModule : IPrefabModule
    {
        [EditorType(EditorType.Animation)]
        public int Animation;

        public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Animator2D
            {
                Speed = Constant.DefaultAnimationSpeed,
                Animation = conversionSystem.GetAnimation(Animation),
            });
        }
    }
}
