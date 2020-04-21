using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;

namespace Chipper.Animation
{
    [RequiresEntityConversion, DisallowMultipleComponent]
    [AddComponentMenu("ECSProxy/Effects/Material Animator Proxy")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class MaterialAnimatorProxy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public List<MaterialAnimationObject> MaterialAnimations;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var animationBuffer = dstManager.AddBuffer<MaterialAnimation>(entity);
            for(int i = 0; i < MaterialAnimations.Count; i++)
            {
                if(MaterialAnimations[i] != null)
                    animationBuffer.Add(MaterialAnimations[i].Component);
            }
        }
    }
}
