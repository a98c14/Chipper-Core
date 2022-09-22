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
    public class AssetCacheEditor : Editor
    {
        GUIStyle horizontalLine;

        // utility method
        void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }

        public override void OnInspectorGUI()
        {
            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;

            var cacheManager = (AssetCacheManager)target;
            EditorUtility.SetDirty(cacheManager);

            GUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hyperion Url:", GUILayout.MaxWidth(80));
            cacheManager.HyperionUrl = GUILayout.TextField(cacheManager.HyperionUrl, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hyperion Websocket Url:", GUILayout.MaxWidth(80));
            cacheManager.HyperionWebSocketUrl= GUILayout.TextField(cacheManager.HyperionWebSocketUrl, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);

            GUILayout.Label("Given path should be relative to Resources folder", EditorStyles.centeredGreyMiniLabel);
            cacheManager.SpritesPath = GUILayout.TextField(cacheManager.SpritesPath);
            cacheManager.ParticleSystemsPath = GUILayout.TextField(cacheManager.ParticleSystemsPath);
            cacheManager.MaterialsPath = GUILayout.TextField(cacheManager.MaterialsPath);
            cacheManager.TrailRenderersPath = GUILayout.TextField(cacheManager.TrailRenderersPath);
            GUILayout.Space(4);
            HorizontalLine(Color.grey);
            GUILayout.Space(4);
            if (GUILayout.Button("Rebuild Cache"))
            {
                cacheManager.RebuildCache();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                var client = new HyperionClient(cacheManager.HyperionUrl);
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
