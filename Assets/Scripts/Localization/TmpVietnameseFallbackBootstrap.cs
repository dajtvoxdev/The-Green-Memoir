using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Ensures Vietnamese glyphs are available in player builds by adding
/// a dynamic OS font fallback before the first scene loads.
///
/// Adds the fallback to BOTH the default TMP font asset's own fallback table
/// AND the global TMP_Settings fallback list for maximum coverage.
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

    private const string FallbackName = "Vietnamese Runtime Fallback";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InstallFallback()
    {
        TMP_FontAsset fallbackFont = CreateDynamicFallback();
        if (fallbackFont == null) return;

        // Strategy 1: Add to the default font asset's own fallback table
        // This is the most reliable approach — TMP always checks per-font fallbacks
        AddToDefaultFontFallback(fallbackFont);

        // Strategy 2: Add to TMP_Settings global fallback list (covers non-default fonts)
        AddToGlobalFallback(fallbackFont);

        Debug.Log($"TmpVietnameseFallbackBootstrap: Installed Vietnamese fallback font.");
    }

    private static TMP_FontAsset CreateDynamicFallback()
    {
        Font osFont = Font.CreateDynamicFontFromOSFont(CandidateFonts, 36);
        if (osFont == null)
        {
            Debug.LogWarning("TmpVietnameseFallbackBootstrap: No candidate OS font found.");
            return null;
        }

        TMP_FontAsset fallback = TMP_FontAsset.CreateFontAsset(osFont);
        if (fallback == null)
        {
            Debug.LogWarning("TmpVietnameseFallbackBootstrap: Failed to create TMP_FontAsset.");
            return null;
        }

        fallback.name = FallbackName;
        fallback.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fallback.isMultiAtlasTexturesEnabled = true;

        return fallback;
    }

    private static void AddToDefaultFontFallback(TMP_FontAsset fallback)
    {
        // Try TMP_Settings.defaultFontAsset first
        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;

        // Fallback: load LiberationSans SDF from Resources (TMP's default location)
        if (defaultFont == null)
        {
            defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        if (defaultFont == null)
        {
            Debug.LogWarning("TmpVietnameseFallbackBootstrap: Default font asset not found.");
            return;
        }

        // Check if already added
        if (defaultFont.fallbackFontAssetTable != null)
        {
            foreach (var fb in defaultFont.fallbackFontAssetTable)
            {
                if (fb != null && fb.name == FallbackName) return;
            }
        }

        if (defaultFont.fallbackFontAssetTable == null)
        {
            defaultFont.fallbackFontAssetTable = new List<TMP_FontAsset>();
        }

        defaultFont.fallbackFontAssetTable.Add(fallback);
    }

    private static void AddToGlobalFallback(TMP_FontAsset fallback)
    {
        if (TMP_Settings.fallbackFontAssets == null)
        {
            // TMP_Settings.fallbackFontAssets is read-only property returning a serialized list;
            // if null, we cannot create it at runtime — per-font fallback (Strategy 1) covers this.
            return;
        }

        foreach (var fb in TMP_Settings.fallbackFontAssets)
        {
            if (fb != null && fb.name == FallbackName) return;
        }

        TMP_Settings.fallbackFontAssets.Add(fallback);
    }
}
