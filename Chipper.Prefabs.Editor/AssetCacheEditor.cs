using Chipper.Prefabs.Network;
using Chipper.Prefabs.Types;
using Chipper.Prefabs.Utils;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Chipper.Prefabs
{
    [CustomEditor(typeof(AssetCacheManager))]
    public class SpriteLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var cacheManager = (AssetCacheManager)target;
            EditorUtility.SetDirty(cacheManager);

            GUILayout.Label("Given path should be relative to Resources folder", EditorStyles.centeredGreyMiniLabel);
            cacheManager.SpritesPath = GUILayout.TextField(cacheManager.SpritesPath);
            cacheManager.ParticleSystemsPath = GUILayout.TextField(cacheManager.ParticleSystemsPath);
            cacheManager.MaterialsPath = GUILayout.TextField(cacheManager.MaterialsPath);
            cacheManager.TrailRenderersPath = GUILayout.TextField(cacheManager.TrailRenderersPath);
            if (GUILayout.Button("Rebuild Cache"))
            {
                cacheManager.RebuildCache();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                var client = new HyperionClient();
                foreach(var cache in cacheManager.Caches)
                    EditorCoroutineUtility.StartCoroutineOwnerless(SyncCache(client, cache));

                EditorCoroutineUtility.StartCoroutineOwnerless(LoadAnimations(client, cacheManager.AnimationCache));
            }
        }

        private static IEnumerator LoadAnimations(HyperionClient client, AnimationCache cache)
        {
            var dco = new DataCoroutineOwnerless(client.GetAnimations());
            yield return dco.Coroutine;
            var animations = (Data.Response.Animation[])dco.Result;
            cache.SaveAnimations(animations);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static IEnumerator SyncCache(HyperionClient client, IUnityAssetCache cache)
        {
            var dco = new DataCoroutineOwnerless(client.SyncAssets(cache.CacheType, cache.Assets));
            yield return dco.Coroutine;
            var assets = (Asset[])dco.Result;
            cache.Sync(assets);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
