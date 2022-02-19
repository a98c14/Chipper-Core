using Chipper.Prefabs.Network;
using Chipper.Prefabs.Types;
using Chipper.Prefabs.Utils;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Chipper.Prefabs
{
    [CustomEditor(typeof(AssetCache))]
    public class SpriteLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var cacheManager = (AssetCache)target;
            EditorUtility.SetDirty(cacheManager);

            GUILayout.Label("Given path should be relative to Resources folder", EditorStyles.centeredGreyMiniLabel);
            cacheManager.SpritesPath = GUILayout.TextField(cacheManager.SpritesPath);
            cacheManager.ParticleSystemsPath = GUILayout.TextField(cacheManager.ParticleSystemsPath);
            cacheManager.MaterialsPath = GUILayout.TextField(cacheManager.MaterialsPath);
            cacheManager.TrailRenderersPath = GUILayout.TextField(cacheManager.TrailRenderersPath);
            if (GUILayout.Button("Rebuild Cache"))
            {
                cacheManager.RebuildCache();
                var client = new HyperionClient();
                foreach(var cache in cacheManager.Caches)
                    EditorCoroutineUtility.StartCoroutineOwnerless(SyncCache(client, cache));
            }
        }

        private static IEnumerator SyncCache(HyperionClient client, AssetCache.IUnityAssetCache cache)
        {
            var dco = new DataCoroutineOwnerless(client.SyncAssets(cache.CacheType, cache.Assets));
            yield return dco.Coroutine;
            var assets = (Asset[])dco.Result;
            cache.Sync(assets);
        }
    }
}
