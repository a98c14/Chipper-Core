using Chipper.Prefabs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Asset Cache")]
public class AssetCache : ScriptableObject, ISerializationCallbackReceiver
{
    public interface IUnityAssetCache
    {
        Asset[] Assets { get; }
        AssetType CacheType { get; }
        void RebuildCache(string path);
        void Sync(Asset[] assets);
    }

    #region AssetCacheGroup
    [Serializable]
    public class UnityAssetCache<T> : IUnityAssetCache where T : UnityEngine.Object
    {
        public AssetType CacheType { get ; private set; }
        public Asset[] Assets { get; private set; }

        /// <summary>
        /// Stores the asset index location with asset id key
        /// </summary>
        private Dictionary<int, int> m_AssetIdMap;

        /// <summary>
        /// Loaded asset objects
        /// </summary>
        private T[] m_AssetObjects;

        // Internal index mapping structures
        private Dictionary<string, int> m_NameMap;
        private Dictionary<long, int> m_InternalIdMap;
        private Dictionary<string, int> m_GuidMap;

        public UnityAssetCache(string resourcePath, AssetType type)
        {
            CacheType = type;
            RebuildCache(resourcePath);
        }

        public void RebuildCache(string resourcePath)
        {
#if UNITY_EDITOR
            m_AssetObjects = Resources.LoadAll<T>(resourcePath);
            m_NameMap = new Dictionary<string, int>(m_AssetObjects.Length);
            m_GuidMap = new Dictionary<string, int>(m_AssetObjects.Length);
            m_InternalIdMap = new Dictionary<long, int>(m_AssetObjects.Length);

            Assets = new Asset[m_AssetObjects.Length];
            for(int i = 0; i < m_AssetObjects.Length; i++)
            {
                var instanceId = m_AssetObjects[i].GetInstanceID();
                var hasGuid = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instanceId, out var guid, out long localId);
                var asset = new Asset
                {
                    InternalId = localId,
                    Name = m_AssetObjects[i].name,
                    InternalGuid = hasGuid ? guid : "",
                    Type = Asset.GetAssetType(typeof(T))
                };

                Assets[i] = asset;
                m_GuidMap[asset.InternalGuid] = i;
                m_InternalIdMap[asset.InternalId] = i;
                m_NameMap[asset.Name] = i;
            }
#endif
        }

        public void Sync(Asset[] dbAssets)
        {
            m_AssetIdMap = new Dictionary<int, int>(m_AssetObjects.Length);
            
            // Try to match the asset id with object index (either with internal id or name)
            foreach (var dbAsset in dbAssets)
            {
                var index = GetAssetIndex(dbAsset);
                Assets[index].InternalId = dbAsset.InternalId;
                Assets[index].Guid = dbAsset.Guid;
                Assets[index].InternalGuid = dbAsset.InternalGuid;
                Assets[index].Name = dbAsset.Name;
                m_AssetIdMap[dbAsset.Id] = index;
            }
        }

        private int GetAssetIndex(Asset asset)
        {
            if (m_InternalIdMap.ContainsKey(asset.InternalId))
                return m_InternalIdMap[asset.InternalId];

            if (m_NameMap.ContainsKey(asset.Name))
                return m_NameMap[asset.Name];

            return -1;
        }
    }
    #endregion

    #region Singleton
    public static AssetCache Main
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = Resources.LoadAll<AssetCache>("").FirstOrDefault();
                Debug.Assert(m_Instance != null, "Asset Cache could not be found!");
            }

            return m_Instance;
        }
    }
    static AssetCache m_Instance = null;
    #endregion
    
    [Header("Resource Local Path")]
    public string MaterialsPath = "Materials";
    public string SpritesPath = "Art/Sprites";
    public string ParticleSystemsPath = "ParticleSystems";
    public string TrailRenderersPath = "TrailRenderers";

    public List<IUnityAssetCache> Caches { get; private set; }

    public UnityAssetCache<Material> MaterialCache { get; private set; }
    public UnityAssetCache<TrailRenderer> TrailRendererCache { get; private set; }
    public UnityAssetCache<ParticleSystem> ParticleSystemCache { get; private set; }
    public UnityAssetCache<Sprite> SpriteCache { get; private set; }

    public void OnAfterDeserialize() { }
    public void OnBeforeSerialize() { }

    #if UNITY_EDITOR
    public void RebuildCache()
    {
        MaterialCache = new UnityAssetCache<Material>(MaterialsPath, AssetType.Material);
        SpriteCache = new UnityAssetCache<Sprite>(SpritesPath, AssetType.Sprite);
        ParticleSystemCache = new UnityAssetCache<ParticleSystem>(ParticleSystemsPath, AssetType.ParticleSystem);
        TrailRendererCache = new UnityAssetCache<TrailRenderer>(TrailRenderersPath, AssetType.TrailSystem);
        Caches = new List<IUnityAssetCache>()
        {
            SpriteCache,
            ParticleSystemCache,
            TrailRendererCache,
            MaterialCache
        };
    }
    #endif
}
