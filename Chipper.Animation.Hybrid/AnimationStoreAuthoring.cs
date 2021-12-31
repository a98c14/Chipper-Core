using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Chipper.Animation
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Animation/Animation Store Authoring")]
    public class AnimationStoreAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public List<SpriteAnimationObject> Animations = new List<SpriteAnimationObject>();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var buffer = dstManager.AddBuffer<AnimationStore>(entity);
            for(int i = 0; i < Animations.Count; i++)
            {
                var animation = Animations[i];
                if (animation == null || animation.Sprites == null)
                    continue;

                buffer.Add(new AnimationStore
                {
                    Value = animation.Component
                });
            }
        }
    }
}