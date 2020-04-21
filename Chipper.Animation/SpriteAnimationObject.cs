#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Chipper.Rendering;

namespace Chipper.Animation
{
    [Serializable]
    [CreateAssetMenu(menuName = "Animation2D")]
    public class SpriteAnimationObject : ScriptableObject
    {        
        public int   FrameCount        => Sprites.Length;
        public float AnimationDuration => FrameCount * Constant.DefaultAnimationSpeed;

        // @IMPORTANT: Do not touch these
        public int Priority; 
        public Sprite[] Sprites;            
        public AnimationTransition TransitionType;

        #if UNITY_EDITOR
        public void Awake()
        {
            var selections = Selection.objects;
            if(selections.Length > 0 && (Sprites == null || Sprites.Length == 0))
            {
                var sprites = new Sprite[selections.Length];
                var isSprite = true;
                for(int i = 0; i < selections.Length; i++)
                {
                    var sprite = selections[i] as Sprite;
                    if(sprite == null)
                    {
                        isSprite = false;
                        break;
                    }
                    sprites[i] = sprite;
                }

                if (isSprite)
                {
                    Sprites = sprites;
                    TransitionType = AnimationTransition.Loop;
                    Debug.Log($"Created new animation from selection! Sprite count: {sprites.Length}");
                }
            }
        }
        #endif

        public Animation2D Component
        {
            get
            {
                Debug.Assert(Sprites != null, $"Animation {name} is null");
                Debug.Assert(Sprites.Length != 0, $"Animation {name} has no sprites!");

                return new Animation2D
                {
                    Animation = CreateBlob(),
                };
            }
        }

        BlobAssetReference<AnimationBlob> CreateBlob()
        {
            var spriteLoader = SpriteLoader.Main;
            using (var builder = new BlobBuilder(Allocator.Temp))
            {
                ref var root = ref builder.ConstructRoot<AnimationBlob>();
                root.Priority = Priority;
                root.TransitionType = TransitionType;

                var sprites = builder.Allocate(ref root.Sprites, Sprites.Length);
                for (int i = 0; i < Sprites.Length; i++)
                    sprites[i] = spriteLoader.GetSpriteID(Sprites[i]);

                return builder.CreateBlobAssetReference<AnimationBlob>(Allocator.Persistent);
            }
        }
    }
}
