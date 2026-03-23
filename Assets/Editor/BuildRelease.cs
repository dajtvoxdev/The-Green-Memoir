using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Batch-friendly Windows release build entry point.
/// Run with:
///   Unity.exe -batchmode -quit -projectPath "<repo>" -executeMethod BuildRelease.PerformBuild
/// </summary>
public static class BuildRelease
{
    private const string OutputDirectory = "Builds";
    private const string OutputFileName = "MoonlitGarden.exe";

    [MenuItem("Tools/Moonlit Garden/Build Windows Release")]
    public static void PerformBuild()
    {
        string releaseVersion = ReleaseVersionSync.TrySyncBundleVersion();
        PlayerSettings.bundleVersion = releaseVersion;

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            throw new System.Exception("No enabled scenes found in Build Settings.");
        }

        Directory.CreateDirectory(OutputDirectory);
        string outputPath = Path.Combine(OutputDirectory, OutputFileName);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        Debug.Log($"BuildRelease: bundleVersion={PlayerSettings.bundleVersion}");
        Debug.Log($"BuildRelease: result={summary.result}, path={summary.outputPath}, size={summary.totalSize}, warnings={summary.totalWarnings}, errors={summary.totalErrors}");

        if (summary.result != BuildResult.Succeeded)
        {
            throw new System.Exception($"Windows build failed with result {summary.result}.");
        }
    }

}
