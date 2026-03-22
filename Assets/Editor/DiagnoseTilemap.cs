using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.IO;

public class DiagnoseTilemap
{
    [MenuItem("MoonlitGarden/Diagnose Tilemaps")]
    public static void Run()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== TILEMAP DIAGNOSIS ===");

        // Check all tilemaps
        var allTilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        sb.AppendLine($"Found {allTilemaps.Length} tilemaps in scene\n");

        foreach (var tm in allTilemaps)
        {
            var bounds = tm.cellBounds;
            int tileCount = 0;
            foreach (var pos in bounds.allPositionsWithin)
            {
                if (tm.HasTile(pos)) tileCount++;
            }
            sb.AppendLine($"{tm.gameObject.name}: tiles={tileCount}, bounds=({bounds.xMin},{bounds.yMin}) to ({bounds.xMax},{bounds.yMax})");
        }

        // Check Walk clips
        sb.AppendLine("\n=== WALK ANIMATION CLIPS ===");
        string[] walkDirs = {"south","north","east","west","south-east","south-west","north-east","north-west"};
        foreach (var dir in walkDirs)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"Assets/Animations/VietnameseFarmer/Walk_{dir}.anim");
            if (clip != null)
            {
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                int spriteKeys = 0;
                string bindingInfo = "";
                foreach (var b in bindings)
                {
                    var kf = AnimationUtility.GetObjectReferenceCurve(clip, b);
                    spriteKeys += kf.Length;
                    bindingInfo = $"path='{b.path}' prop='{b.propertyName}' type={b.type.Name}";
                }
                sb.AppendLine($"Walk_{dir}: spriteKeys={spriteKeys}, loop={clip.isLooping}, {bindingInfo}");
            }
            else
            {
                sb.AppendLine($"Walk_{dir}: CLIP NOT FOUND!");
            }
        }

        // Write to file
        string path = "Assets/Editor/diagnose_output.txt";
        File.WriteAllText(path, sb.ToString());
        Debug.LogWarning($"[Diagnose] Output written to {path}");
        AssetDatabase.Refresh();
    }
}
