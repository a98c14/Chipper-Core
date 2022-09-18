using Chipper.Prefabs.Types;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chipper.Prefabs
{
    public interface IUnityAssetCache
    {
        Asset[] Assets { get; }
        AssetType CacheType { get; }
        void RebuildCache(string path);
        void Sync(Asset[] assets);
        object GetAsset(int assetId);
        void SerializeCache();
    }

    [Serializable]
    public class AssetCache<T> : IUnityAssetCache where T : UnityEngine.Object
    {
        public AssetType CacheType { get; private set; }
        public Asset[] Assets { get; private set; }

        /// <summary>
        /// Stores the asset index location with asset id key
        /// </summary>
        private Dictionary<int, int> m_AssetIdMap;

        /// <summary>
        /// Loaded asset objects
        /// </summary>
        [SerializeField]
        private T[] m_AssetObjects;

        [SerializeField]
        private int[] m_IndexAssetIds;

        // Internal index mapping structures
        private Dictionary<string, int> m_NameMap;
        private Dictionary<long, int> m_InternalIdMap;
        private Dictionary<string, int> m_GuidMap;

        public AssetCache(string resourcePath, AssetType type)
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
            for (int i = 0; i < m_AssetObjects.Length; i++)
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
            m_AssetIdMap = new Dictionary<int, int>(dbAssets.Length);

            // Try to match the asset id with object index (either with internal id or name)
            m_IndexAssetIds = new int[dbAssets.Length];
            foreach (var dbAsset in dbAssets)
            {
                var index = GetAssetIndex(dbAsset);
                if(index == -1)
                {
                    Debug.Log($"Could not find asset for {dbAsset.Name}");
                    continue;
                }
                Assets[index].InternalId = dbAsset.InternalId;
                Assets[index].Guid = dbAsset.Guid;
                Assets[index].InternalGuid = dbAsset.InternalGuid;
                Assets[index].Name = dbAsset.Name;
                m_AssetIdMap[dbAsset.Id] = index;
                m_IndexAssetIds[index] = dbAsset.Id;
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

        public object GetAsset(int assetId)
        {
            Debug.Assert(assetId >= 0 && m_AssetIdMap.ContainsKey(assetId), $"Asset Id: {assetId} is not valid. Cache Type: {CacheType}, Class: {nameof(T)}");
            var index = m_AssetIdMap[assetId];
            return m_AssetObjects[index];
        }

        public int GetAssetIndex(int assetId)
        {
            return m_AssetIdMap[assetId];
        }

        public void SerializeCache()
        {
            m_NameMap = new Dictionary<string, int>(m_AssetObjects.Length);
            m_GuidMap = new Dictionary<string, int>(m_AssetObjects.Length);
            m_InternalIdMap = new Dictionary<long, int>(m_AssetObjects.Length);
            m_NameMap.Clear();
            m_GuidMap.Clear();
            m_InternalIdMap.Clear();
            m_AssetIdMap.Clear();

            for (int i = 0; i < m_IndexAssetIds.Length; i++)
            {
                var id = m_IndexAssetIds[i];
                var asset = Assets[id];
                m_NameMap[asset.Name] = i;
                m_InternalIdMap[asset.InternalId] = i;
                m_AssetIdMap[id] = i;
                m_GuidMap[asset.Guid] = i;
            }
        }
    }
}