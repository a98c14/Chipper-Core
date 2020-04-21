using System;
using UnityEngine;
using UnityEditor;

namespace Chipper.Animation
{
    [CustomEditor(typeof(SpriteAnimationObject))]
    class SpriteAnimationEditor : Editor
    {
        GUIStyle    m_HeaderLabelStyle;
        GUIStyle    m_CenterLabelStyle;
        Texture2D[] m_PreviewTextures;
        Vector2     m_ScrollPosition;

        public void OnEnable()
        {
            var spriteAnimation = (SpriteAnimationObject)target;
            m_PreviewTextures   = new Texture2D[20];
            EditorUtility.SetDirty(spriteAnimation);
        }

        public override void OnInspectorGUI()
        {
            EditorUtils.Init();
            var animation = (SpriteAnimationObject)target;        
            m_HeaderLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            m_CenterLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };        

            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Sprites", EditorUtils.HeaderStyle);
            EditorGUILayout.ObjectField(null, typeof(Sprite), false);

            EditorGUILayout.LabelField($"Frame count: { (animation.Sprites != null ? animation.FrameCount : 0) }", EditorUtils.HeaderStyle);

            if (EditorGUI.EndChangeCheck())
            {
                var sprites = Array.ConvertAll(Selection.objects, item => (Sprite)item);
                animation.Sprites = sprites;

                // Ensure preview texture cache size is enough
                if(sprites.Length > m_PreviewTextures.Length)
                {
                    m_PreviewTextures = new Texture2D[sprites.Length];
                }

                // Clear preview images
                for(int i = 0; i < m_PreviewTextures.Length; i++)
                {
                    m_PreviewTextures[i] = null;
                }
            }

            if (animation.Sprites != null)
            {
                var currentWidth = EditorGUIUtility.currentViewWidth;
                var desiredHeight =  currentWidth / animation.Sprites.Length;
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.Width(currentWidth - 25), GUILayout.Height(90));
                EditorGUILayout.BeginHorizontal();

                // Ensure preview texture cache size is enough
                if(animation.Sprites.Length > m_PreviewTextures.Length)
                {
                    m_PreviewTextures = new Texture2D[animation.Sprites.Length];
                }

                for (int i = 0; i < animation.Sprites.Length; i++)
                {
                    EditorUtils.DrawFixedHeightSpriteField("", animation.Sprites[i], 60, ref m_PreviewTextures[i]);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.LabelField("No sprite selected!", m_CenterLabelStyle);
            }
            EditorGUILayout.EndVertical();

            if (animation.Sprites != null)
            {
                if (GUILayout.Button("Show in Folder"))
                {
                    var path = AssetDatabase.GetAssetPath(animation.Sprites[0]);
                    path = path.Replace(@"/", @"\");
                    System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
                }
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Transition Settings:", m_HeaderLabelStyle);
            animation.TransitionType = (AnimationTransition)EditorGUILayout.EnumPopup("Transition Type:", animation.TransitionType);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Priority:", m_HeaderLabelStyle);
            EditorGUILayout.LabelField("Lower priority animations can't override higher priority animations.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Default Animations: 0", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Attack Animations: 1", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Hit and Uncancellable Attack Animations: 2", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Death Animations: 3", EditorStyles.centeredGreyMiniLabel);

            animation.Priority = GUILayout.Toolbar(animation.Priority, new[] { "0", "1", "2", "3" });
            EditorGUILayout.EndVertical();
        }
    }
}
