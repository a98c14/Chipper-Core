using Chipper.Prefabs.Types;
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Chipper.Prefabs
{
    [Serializable]
    public class AnimationCache 
    {
        /// <summary>
        /// Stores the asset index location with asset id key
        /// </summary>
        private Dictionary<int, int> m_AssetIdMap;

        [SerializeField]
        private Data.Response.Animation[] m_Animations;

        [SerializeField]
        private int[] m_IndexAssetIds;

        // Internal index mapping structures
        private Dictionary<string, int> m_NameMap;

        public void SaveAnimations(Data.Response.Animation[] animations)
        {
            m_Animations = animations;
            m_IndexAssetIds = new int[animations.Length];
            m_AssetIdMap = new Dictionary<int, int>(animations.Length);
            m_NameMap = new Dictionary<string, int>(animations.Length);

            for(int i = 0; i < m_Animations.Length; i++)
            {
                var anim = m_Animations[i];
                m_AssetIdMap[anim.AssetId] = i;
                m_NameMap[anim.Name] = i;
                m_IndexAssetIds[i] = anim.AssetId;
            }
        }

        private int GetAnimationIndex(Asset asset)
        {
            if (m_NameMap.ContainsKey(asset.Name))
                return m_NameMap[asset.Name];

            return -1;
        }

        public Data.Response.Animation GetAnimation(int assetId)
        {
            Debug.Assert(assetId >= 0 && m_AssetIdMap.ContainsKey(assetId), $"Asset Id: {assetId} is not valid. AnimationCache");
            var index = m_AssetIdMap[assetId];
            return m_Animations[index];
        }

        public int GetAnimationIndex(int assetId)
        {
            return m_AssetIdMap[assetId];
        }

        public void SerializeCache()
        {
            if(m_NameMap == null)
                m_NameMap = new Dictionary<string, int>(m_Animations.Length);

            if(m_AssetIdMap == null)
                m_AssetIdMap = new Dictionary<int, int>(m_Animations.Length);

            m_NameMap.Clear();
            m_AssetIdMap.Clear();

            for (int i = 0; i < m_IndexAssetIds.Length; i++)
            {
                var id = m_IndexAssetIds[i];
                var animation = m_Animations[i];
                m_NameMap[animation.Name] = i;
                m_AssetIdMap[id] = i;
            }
        }
    }
}