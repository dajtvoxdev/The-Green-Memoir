using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ReleaseVersionSync
{
    private const string ReleaseManifestPath = "web/src/lib/game-release.ts";

    static ReleaseVersionSync()
    {
        TrySyncBundleVersion();
    }

    public static string TrySyncBundleVersion()
    {
        string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), ReleaseManifestPath);
        if (!File.Exists(manifestPath))
        {
            return PlayerSettings.bundleVersion;
        }

        string manifestSource = File.ReadAllText(manifestPath);
        Match match = Regex.Match(
            manifestSource,
            @"CURRENT_GAME_VERSION_NUMBER\s*=\s*'(?<version>[^']+)'",
            RegexOptions.Multiline);

        if (!match.Success)
        {
            return PlayerSettings.bundleVersion;
        }

        string version = match.Groups["version"].Value.Trim();
        if (version.StartsWith("v"))
        {
            version = version.Substring(1);
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            return PlayerSettings.bundleVersion;
        }

        if (PlayerSettings.bundleVersion != version)
        {
            PlayerSettings.bundleVersion = version;
            AssetDatabase.SaveAssets();
            Debug.Log($"ReleaseVersionSync: bundleVersion updated to {version}");
        }

        return PlayerSettings.bundleVersion;
    }
}
