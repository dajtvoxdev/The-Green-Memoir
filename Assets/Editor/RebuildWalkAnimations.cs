using UnityEngine;
using UnityEditor;

/// <summary>
/// Rebuilds all Walk and Idle animation clips for the Vietnamese Farmer character.
/// Fixes broken sprite references after changing spriteMode from Multiple to Single.
/// </summary>
public class RebuildWalkAnimations
{
    private const string WALK_BASE = "Assets/A_Vietnamese_farmer_from_the_countryside_standing/animations/walking-6-frames";
    private const string IDLE_BASE = "Assets/A_Vietnamese_farmer_from_the_countryside_standing/animations/breathing-idle";
    private const string ANIM_PATH = "Assets/Animations/VietnameseFarmer";
    private const int WALK_FRAMES = 6;
    private const int IDLE_FRAMES = 4;
    private const int SAMPLE_RATE = 12;

    private static readonly string[] Directions = new string[]
    {
        "east", "north-east", "north", "north-west",
        "west", "south-west", "south", "south-east"
    };

    [MenuItem("MoonlitGarden/Rebuild Walk Animations")]
    public static void RebuildWalk()
    {
        int rebuilt = RebuildClips("Walk", WALK_BASE, WALK_FRAMES);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Rebuild] Done! Walk: " + rebuilt + "/8 clips.");
        EditorUtility.DisplayDialog("Rebuild Walk", "Rebuilt " + rebuilt + "/8 Walk clips.", "OK");
    }

    [MenuItem("MoonlitGarden/Rebuild Idle Animations")]
    public static void RebuildIdle()
    {
        int rebuilt = RebuildClips("Idle", IDLE_BASE, IDLE_FRAMES);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Rebuild] Done! Idle: " + rebuilt + "/8 clips.");
        EditorUtility.DisplayDialog("Rebuild Idle", "Rebuilt " + rebuilt + "/8 Idle clips.", "OK");
    }

    [MenuItem("MoonlitGarden/Rebuild All Animations")]
    public static void RebuildAll()
    {
        int walkCount = RebuildClips("Walk", WALK_BASE, WALK_FRAMES);
        int idleCount = RebuildClips("Idle", IDLE_BASE, IDLE_FRAMES);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        string msg = "Walk: " + walkCount + "/8, Idle: " + idleCount + "/8";
        Debug.Log("[Rebuild] Done! " + msg);
        EditorUtility.DisplayDialog("Rebuild All Animations", msg, "OK");
    }

    private static int RebuildClips(string prefix, string spritesBase, int frameCount)
    {
        int rebuilt = 0;

        foreach (var dir in Directions)
        {
            string clipPath = ANIM_PATH + "/" + prefix + "_" + dir + ".anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                Debug.LogWarning("[Rebuild] Clip not found: " + clipPath);
                continue;
            }

            Sprite[] sprites = new Sprite[frameCount];
            bool allLoaded = true;
            for (int i = 0; i < frameCount; i++)
            {
                string spritePath = spritesBase + "/" + dir + "/frame_00" + i + ".png";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprites[i] == null)
                {
                    Debug.LogError("[Rebuild] Missing sprite: " + spritePath);
                    allLoaded = false;
                }
            }
            if (!allLoaded) continue;

            // Clear existing curves
            var oldBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var b in oldBindings)
            {
                AnimationUtility.SetObjectReferenceCurve(clip, b, null);
            }

            // Create keyframes
            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = (float)i / SAMPLE_RATE,
                    value = sprites[i]
                };
            }

            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            clip.frameRate = SAMPLE_RATE;
            EditorUtility.SetDirty(clip);
            rebuilt++;
            Debug.Log("[Rebuild] OK: " + prefix + "_" + dir + " - " + frameCount + " frames");
        }

        return rebuilt;
    }
}
