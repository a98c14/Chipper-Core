using Chipper.Prefabs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Chipper.Animation;
using Unity.Collections;
using Chipper.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chipper.Prefabs
{
    [CreateAssetMenu(menuName = "Asset Cache")]
    public class AssetCacheManager : ScriptableObject, ISerializationCallbackReceiver
    {
        #region Singleton
        public static AssetCacheManager Main
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = Resources.LoadAll<AssetCacheManager>("").FirstOrDefault();
                    Debug.Assert(m_Instance != null, "Asset Cache could not be found!");
                }

                return m_Instance;
            }
        }
        static AssetCacheManager m_Instance = null;
        #endregion

        public string HyperionUrl = "";
        public string HyperionWebSocketUrl = "";

        [Header("Resource Local Path")]
        public string MaterialsPath = "Materials";
        public string SpritesPath = "Art/Sprites";
        public string ParticleSystemsPath = "ParticleSystems";
        public string TrailRenderersPath = "TrailRenderers";

        public List<IUnityAssetCache> Caches { get; private set; }
        public Dictionary<Type, IUnityAssetCache> CacheTypeMap { get; private set; }

        public AssetCache<Material> MaterialCache { get; private set; }
        public AssetCache<TrailRenderer> TrailRendererCache { get; private set; }
        public AssetCache<ParticleSystem> ParticleSystemCache { get; private set; }
        public AssetCache<Sprite> SpriteCache { get; private set; }
        public AnimationCache AnimationCache { get; private set; }

        public void OnAfterDeserialize()
        {
            if (Caches is null) return;
            foreach (var cache in Caches)
            {
                cache.SerializeCache();
            }
        }

        public void OnBeforeSerialize()
        {

        }

        public Data.Response.Animation GetAnimation(int assetId)
        {
            return AnimationCache.GetAnimation(assetId);
        }

        public T GetAsset<T>(int assetId)
        {
            Debug.Assert(CacheTypeMap.ContainsKey(typeof(T)));
            var cache = CacheTypeMap[typeof(T)];
            return (T)cache.GetAsset(assetId);
        }

#if UNITY_EDITOR
        public void RebuildCache()
        {
            CacheTypeMap = new Dictionary<Type, IUnityAssetCache> {
                { typeof(Sprite), SpriteCache },
                { typeof(Material),MaterialCache },
                { typeof(ParticleSystem), ParticleSystemCache },
                { typeof(TrailRenderer), TrailRendererCache},
            };

            MaterialCache = new AssetCache<Material>(MaterialsPath, AssetType.Material);
            SpriteCache = new AssetCache<Sprite>(SpritesPath, AssetType.Sprite);
            ParticleSystemCache = new AssetCache<ParticleSystem>(ParticleSystemsPath, AssetType.ParticleSystem);
            TrailRendererCache = new AssetCache<TrailRenderer>(TrailRenderersPath, AssetType.TrailSystem);
            AnimationCache = new AnimationCache();
            
            Caches = new List<IUnityAssetCache>()
            {
                SpriteCache,
                ParticleSystemCache,
                TrailRendererCache,
                MaterialCache
            };

            Debug.Log("Rebuilt cache successfully!");
        }


        public Animation2D CreateAnimationBlob(Data.Response.Animation animation)
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<AnimationBlob>();
            root.ID = GetHashCode();
            root.Priority = animation.Priority;
            root.TransitionType = (AnimationTransition)animation.TransitionType;

            var sprites = builder.Allocate(ref root.Sprites, animation.Sprites.Length);
            for (int i = 0; i < animation.Sprites.Length; i++)
                sprites[i] = new SpriteID { Value = SpriteCache.GetAssetIndex(animation.Sprites[i]) };

            return new Animation2D
            {
                Animation = builder.CreateBlobAssetReference<AnimationBlob>(Allocator.Persistent)
            };
        }
#endif
    }
}