using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

public static class EditorUtils
{
    /// ### PUBLIC FIELDS ###
    public static GUIStyle HeaderStyle;
    public static GUIStyle CenterLabelStyle;
    public static GUIStyle BoldTextStyle;
    public static GUIStyle BoldFoldoutStyle;

    public static void Init()
    {
        if (HeaderStyle != null) return;

        HeaderStyle      = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        BoldTextStyle    = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        CenterLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        BoldFoldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
    }

    public static Sprite DrawScalableSpriteField(string label, Sprite sprite, ref int targetSizeSlider, ref Texture2D scaledTexture)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label);
        sprite = (Sprite)EditorGUILayout.ObjectField(sprite, typeof(Sprite), false);
        EditorGUILayout.EndHorizontal();

        if (sprite != null)
        {
            targetSizeSlider = EditorGUILayout.IntSlider("Scaled Size:", targetSizeSlider, 25, 400);
        }
        if (EditorGUI.EndChangeCheck())
        {
            if (sprite != null)
            {
                scaledTexture = ScaleTextureFixedWidth(sprite.texture, sprite.rect, targetSizeSlider);
            }
        }

        if(scaledTexture == null && sprite != null)
        {
            scaledTexture = ScaleTextureFixedWidth(sprite.texture, sprite.rect, targetSizeSlider);
        }

        if (scaledTexture != null)
        {
            GUILayout.Label(scaledTexture, CenterLabelStyle, GUILayout.Height(scaledTexture.height));
        }

        EditorGUILayout.EndVertical();
        return sprite;
    }

    public static Sprite DrawFixedWidthSpriteField(string label, Sprite sprite, int targetWidth, ref Texture2D scaledTexture)
    {
        EditorGUILayout.BeginVertical("box");
        if (scaledTexture == null && sprite != null)
        {
            scaledTexture = ScaleTextureFixedWidth(sprite.texture, sprite.rect, targetWidth);
        }

        if (scaledTexture != null)
        {
            GUILayout.Label(scaledTexture, CenterLabelStyle, GUILayout.Height(scaledTexture.height));
        }

        EditorGUILayout.EndVertical();
        return sprite;
    }

    public static Sprite DrawFixedHeightSpriteField(string label, Sprite sprite, int targetHeight, ref Texture2D scaledTexture)
    {
        EditorGUILayout.BeginVertical("box");
        if (scaledTexture == null && sprite != null)
        {
            scaledTexture = ScaleTextureFixedHeight(sprite.texture, sprite.rect, targetHeight);
        }

        if (scaledTexture != null)
        {
            GUILayout.Label(scaledTexture, CenterLabelStyle, GUILayout.Height(scaledTexture.height));
        }

        EditorGUILayout.EndVertical();
        return sprite;
    }

    public static Texture2D ScaleTextureFixedWidth(Texture2D source, Rect textureRect, int targetWidth)
    {
        var targetHeight = (int)(targetWidth * textureRect.height / textureRect.width);
        var result = new Texture2D(targetWidth, targetHeight, source.format, false);

        var scaleX = result.width / textureRect.width;
        var scaleY = result.height / textureRect.height;

        scaleX = Mathf.Max(1, scaleX);
        scaleY = Mathf.Max(1, scaleY);
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixel((int)(textureRect.xMin + j / scaleX), (int)(textureRect.yMin + i / scaleY));
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }

    public static Texture2D ScaleTextureFixedHeight(Texture2D source, Rect textureRect, int targetHeight)
    {
        var targetWidth = (int)(targetHeight * textureRect.width / textureRect.height);
        var result = new Texture2D(targetWidth, targetHeight, source.format, false);

        var scaleX = result.width / textureRect.width;
        var scaleY = result.height / textureRect.height;

        scaleX = Mathf.Max(1, scaleX);
        scaleY = Mathf.Max(1, scaleY);
        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                var newColor = source.GetPixel((int)(textureRect.xMin + j / scaleX), (int)(textureRect.yMin + i / scaleY));
                result.SetPixel(j, i, newColor);
            }
        }
        result.Apply();
        return result;
    }
}
