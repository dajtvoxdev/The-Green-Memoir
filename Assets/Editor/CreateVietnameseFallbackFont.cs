using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// Creates a permanent Vietnamese fallback TMP font asset from Arial.ttf in the project.
/// The asset is saved to disk and wired as a fallback on LiberationSans SDF,
/// so Vietnamese diacritics render correctly in builds.
///
/// Run via: Tools > Moonlit Garden > Create Vietnamese Fallback Font
/// </summary>
public static class CreateVietnameseFallbackFont
{
    private const string FontSourcePath = "Assets/Fonts/Arial.ttf";
    private const string OutputFolder = "Assets/TextMesh Pro/Resources/Fonts & Materials";
    private const string AssetName = "Vietnamese Fallback SDF";
    private const string AssetPath = OutputFolder + "/" + AssetName + ".asset";

    [MenuItem("Tools/Moonlit Garden/Create Vietnamese Fallback Font")]
    public static void Execute()
    {
        // 1. Load the project font file (Arial.ttf copied from Windows/Fonts)
        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(FontSourcePath);

        if (sourceFont == null)
        {
            Debug.LogWarning($"CreateVietnameseFallbackFont: Font not found at {FontSourcePath}, trying OS font...");
            sourceFont = Font.CreateDynamicFontFromOSFont("Arial", 36);
        }

        if (sourceFont == null)
        {
            Debug.LogError("CreateVietnameseFallbackFont: No font source available!");
            return;
        }

        Debug.Log($"CreateVietnameseFallbackFont: Using font '{sourceFont.name}'");

        // 2. Create TMP_FontAsset — try simple overload first, then parameterized
        TMP_FontAsset fallbackFont = TMP_FontAsset.CreateFontAsset(sourceFont);

        if (fallbackFont == null)
        {
            Debug.LogError("CreateVietnameseFallbackFont: TMP_FontAsset.CreateFontAsset returned null!");
            return;
        }

        fallbackFont.name = AssetName;
        fallbackFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fallbackFont.isMultiAtlasTexturesEnabled = true;

        // 3. Save as permanent asset
        if (!Directory.Exists(OutputFolder))
        {
            Directory.CreateDirectory(OutputFolder);
        }

        // Delete old asset if exists
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath) != null)
        {
            AssetDatabase.DeleteAsset(AssetPath);
        }

        AssetDatabase.CreateAsset(fallbackFont, AssetPath);

        // Save atlas texture and material as sub-assets
        if (fallbackFont.atlasTexture != null)
        {
            fallbackFont.atlasTexture.name = AssetName + " Atlas";
            AssetDatabase.AddObjectToAsset(fallbackFont.atlasTexture, fallbackFont);
        }

        if (fallbackFont.material != null)
        {
            fallbackFont.material.name = AssetName + " Material";
            AssetDatabase.AddObjectToAsset(fallbackFont.material, fallbackFont);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"CreateVietnameseFallbackFont: Saved font asset at {AssetPath}");

        // 4. Wire as fallback on LiberationSans SDF
        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        if (defaultFont == null)
        {
            Debug.LogWarning("CreateVietnameseFallbackFont: LiberationSans SDF not found!");
            return;
        }

        // Reload saved asset
        TMP_FontAsset savedFallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
        if (savedFallback == null)
        {
            Debug.LogError("CreateVietnameseFallbackFont: Could not reload saved asset!");
            return;
        }

        if (defaultFont.fallbackFontAssetTable == null)
        {
            defaultFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
        }

        // Remove existing Vietnamese fallback if any, then insert at front
        defaultFont.fallbackFontAssetTable.RemoveAll(f => f != null && f.name == AssetName);
        defaultFont.fallbackFontAssetTable.Insert(0, savedFallback);

        EditorUtility.SetDirty(defaultFont);
        AssetDatabase.SaveAssets();

        Debug.Log($"CreateVietnameseFallbackFont: Wired as fallback on LiberationSans SDF. " +
                  $"Fallback count: {defaultFont.fallbackFontAssetTable.Count}");

        // 5. Also wire to TMP Settings global fallback
        string settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        TMP_Settings settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>(settingsPath);
        if (settings != null)
        {
            var so = new SerializedObject(settings);
            var fallbackList = so.FindProperty("m_fallbackFontAssets");

            if (fallbackList != null)
            {
                bool alreadyExists = false;
                for (int i = 0; i < fallbackList.arraySize; i++)
                {
                    if (fallbackList.GetArrayElementAtIndex(i).objectReferenceValue == savedFallback)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    fallbackList.InsertArrayElementAtIndex(0);
                    fallbackList.GetArrayElementAtIndex(0).objectReferenceValue = savedFallback;
                    so.ApplyModifiedProperties();
                    Debug.Log("CreateVietnameseFallbackFont: Added to TMP Settings global fallback.");
                }
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        Debug.Log("CreateVietnameseFallbackFont: Complete! Vietnamese will render in builds.");
    }
}
