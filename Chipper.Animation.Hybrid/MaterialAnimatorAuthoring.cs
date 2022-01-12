using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;
using Chipper.Prefabs;
using Chipper.Prefabs.Attributes;
using Chipper.Prefabs.Types;

namespace Chipper.Animation
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Chipper/Effects/Material Animator Proxy")]
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

    public class MaterialAnimatorModule : IPrefabModule
    {
        [EditorType(EditorType.MaterialAnimation)]
        public List<int> MaterialAnimations;

        public void Convert(Entity entity, EntityManager dstManager, IPrefabConversionSystem conversionSystem)
        {
            var animationBuffer = dstManager.AddBuffer<MaterialAnimation>(entity);
            for (int i = 0; i < MaterialAnimations.Count; i++)
            {
                var anim = MaterialAnimations[i];
                animationBuffer.Add(conversionSystem.GetMaterialAnimation(anim));
            }
        }
    }
}
