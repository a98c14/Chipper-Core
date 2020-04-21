using UnityEngine;
using UnityEditor;

namespace Chipper.Rendering
{
    [CustomEditor(typeof(SpriteLoader))]
    public class SpriteLoaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var spriteLoader = (SpriteLoader)target;
            EditorUtility.SetDirty(spriteLoader);

            GUILayout.Label("Given path should be relative to Resources folder", EditorStyles.centeredGreyMiniLabel);
            spriteLoader.SpritePath = GUILayout.TextField(spriteLoader.SpritePath);
            if (GUILayout.Button("Rebuild Sprite Cache"))
                spriteLoader.RebuildCache();
        }
    }
}
