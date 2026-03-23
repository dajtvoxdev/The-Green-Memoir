using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Ensures Vietnamese glyphs are available in player builds by adding
/// a dynamic OS font fallback before the first scene loads.
/// </summary>
public static class TmpVietnameseFallbackBootstrap
{
    private static readonly string[] CandidateFonts =
    {
        "Segoe UI",
        "Arial",
        "Tahoma",
        "Calibri",
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InstallFallback()
    {
        TMP_Settings settings = TMP_Settings.instance;
        if (settings == null)
        {
            Debug.LogWarning("TmpVietnameseFallbackBootstrap: TMP Settings not available.");
            return;
        }

        if (TMP_Settings.fallbackFontAssets != null &&
            TMP_Settings.fallbackFontAssets.Any(font => font != null && font.name == "Vietnamese Runtime Fallback"))
        {
            return;
        }

        Font osFont = Font.CreateDynamicFontFromOSFont(CandidateFonts, 36);
        if (osFont == null)
        {
            Debug.LogWarning("TmpVietnameseFallbackBootstrap: Could not create OS fallback font.");
            return;
        }

        TMP_FontAsset fallbackFont = TMP_FontAsset.CreateFontAsset(osFont);
        if (fallbackFont == null)
        {
            Debug.LogWarning("TmpVietnameseFallbackBootstrap: Could not create TMP fallback font.");
            return;
        }

        fallbackFont.name = "Vietnamese Runtime Fallback";
        fallbackFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fallbackFont.isMultiAtlasTexturesEnabled = true;

        TMP_Settings.fallbackFontAssets.Add(fallbackFont);
        Debug.Log($"TmpVietnameseFallbackBootstrap: Added runtime fallback font from '{osFont.name}'.");
    }
}
