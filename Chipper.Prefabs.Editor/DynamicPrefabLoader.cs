using Chipper.Prefabs.Network;
using Chipper.Prefabs.Types;
using Chipper.Prefabs.Utils;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace Chipper.Prefabs
{
    public class DynamicPrefabLoader : EditorWindow
    {
        [MenuItem("Dynamic Prefabs/Generate Animations")]
        public static void GenerateAnimations()
        {
            var client = new HyperionClient();
            EditorCoroutineUtility.StartCoroutineOwnerless(client.GenerateAnimations());
        }

        [MenuItem("Dynamic Prefabs/Upload Modules")]
        public static void SavePrefabs()
        {
            // Sync prefab structures with hyperion.
            var client = new HyperionClient();
            var jsons = AssetManager.GetModuleJsonData();
            foreach(var json in jsons)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(client.CreatePrefabModule(json));
            }
        }

        [MenuItem("Dynamic Prefabs/Upload Sprites")]
        public static void UploadSprites()
        {
            var path = @"F:\work\RogueChampions\RogueChampions\Assets\Resources\Art\Sprites\";
            var client = new HyperionClient();
            var textures = AssetManager.GetSpriteData(path);
            foreach (var texture in textures)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(client.UploadSprites(texture));
            }
        }
    }
}
