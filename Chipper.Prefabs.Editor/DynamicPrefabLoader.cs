using Chipper.Prefabs.Network;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace Chipper.Prefabs.Editor
{
    public class DynamicPrefabLoader : EditorWindow
    {
        [MenuItem("Chipper/Prefabs/Save All")]
        public static void SavePrefabs()
        {
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
            var path = @"D:\Work\RogueChampions\RogueChampions\Assets\Resources\Art\Sprites\";
            var client = new HyperionClient();
            var textures = AssetManager.GetSpriteData(path);
            foreach (var texture in textures)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(client.UploadSprites(texture));
            }
        }
    }
}
