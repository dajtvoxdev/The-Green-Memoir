using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Editor utility to:
/// 1. Assign waterTilemap reference on PlayerMovement_Mouse
/// 2. Configure 8-direction walking animation clips from walking-6-frames sprites
/// </summary>
public class SetupWaterAndAnimations
{
    [MenuItem("MoonlitGarden/Setup Water + Walking Animations")]
    public static void Run()
    {
        AssignWaterTilemap();
        ConfigureWalkingAnimations();
        Debug.Log("[Setup] All done! Save the scene.");
    }

    [MenuItem("MoonlitGarden/1. Assign Water Tilemap")]
    public static void AssignWaterTilemap()
    {
        var player = GameObject.Find("Player");
        if (player == null) { Debug.LogError("Player not found"); return; }

        var waterGO = GameObject.Find("Tilemap_Water");
        if (waterGO == null) { Debug.LogError("Tilemap_Water not found"); return; }

        var mouseCtrl = player.GetComponent<PlayerMovement_Mouse>();
        if (mouseCtrl == null) { Debug.LogError("PlayerMovement_Mouse not found on Player"); return; }

        var waterTilemap = waterGO.GetComponent<Tilemap>();
        if (waterTilemap == null) { Debug.LogError("Tilemap component not found on Tilemap_Water"); return; }

        mouseCtrl.waterTilemap = waterTilemap;

        // Also assign grass tilemap
        var grassGO = GameObject.Find("Tilemap_Grass");
        if (grassGO != null)
        {
            mouseCtrl.grassTilemap = grassGO.GetComponent<Tilemap>();
            Debug.Log("[Setup] grassTilemap assigned on PlayerMovement_Mouse -> Tilemap_Grass");
        }

        EditorUtility.SetDirty(mouseCtrl);
        Debug.Log("[Setup] waterTilemap assigned on PlayerMovement_Mouse -> Tilemap_Water");

        // Also assign on keyboard PlayerMovement
        var keyboardCtrl = player.GetComponent<PlayerMovement>();
        if (keyboardCtrl != null)
        {
            keyboardCtrl.waterTilemap = waterTilemap;
            EditorUtility.SetDirty(keyboardCtrl);
            Debug.Log("[Setup] waterTilemap assigned on PlayerMovement (keyboard) -> Tilemap_Water");
        }
    }

    [MenuItem("MoonlitGarden/2. Configure Walking Animations")]
    public static void ConfigureWalkingAnimations()
    {
        string[] directions = { "east", "west", "north", "south", "north-east", "north-west", "south-east", "south-west" };
        string spritesRoot = "Assets/A_Vietnamese_farmer_from_the_countryside_standing/animations/walking-6-frames";
        string animsRoot = "Assets/Animations/VietnameseFarmer";

        int configured = 0;

        foreach (var dir in directions)
        {
            string animPath = $"{animsRoot}/Walk_{dir}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);

            if (clip == null)
            {
                Debug.LogWarning($"[Setup] Animation clip not found: {animPath}, creating new one");
                clip = new AnimationClip();
                clip.name = $"Walk_{dir}";
                AssetDatabase.CreateAsset(clip, animPath);
            }

            // Load sprites for this direction
            string spriteDir = $"{spritesRoot}/{dir}";
            var sprites = new List<Sprite>();

            for (int i = 0; i < 6; i++)
            {
                string framePath = $"{spriteDir}/frame_{i:D3}.png";
                // Load all sprites from the texture (might be single sprite or spritesheet)
                var allAtPath = AssetDatabase.LoadAllAssetsAtPath(framePath);
                Sprite sprite = null;
                foreach (var obj in allAtPath)
                {
                    if (obj is Sprite s)
                    {
                        sprite = s;
                        break;
                    }
                }

                if (sprite == null)
                {
                    // Try loading as Texture2D and getting the default sprite
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(framePath);
                    if (tex != null)
                    {
                        // Ensure texture is set to Sprite mode
                        var importer = AssetImporter.GetAtPath(framePath) as TextureImporter;
                        if (importer != null && importer.textureType != TextureImporterType.Sprite)
                        {
                            importer.textureType = TextureImporterType.Sprite;
                            importer.spriteImportMode = SpriteImportMode.Single;
                            importer.SaveAndReimport();
                        }
                        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
                    }
                }

                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
                else
                {
                    Debug.LogWarning($"[Setup] Sprite not found: {framePath}");
                }
            }

            if (sprites.Count == 0)
            {
                Debug.LogWarning($"[Setup] No sprites found for direction: {dir}");
                continue;
            }

            // Create sprite animation curve
            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Count];
            float frameDuration = 1f / 12f; // 12 FPS

            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameDuration,
                    value = sprites[i]
                };
            }

            // Clear existing curves
            clip.ClearCurves();

            // Set the sprite keyframes
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            // Configure clip settings
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            clip.frameRate = 12;

            EditorUtility.SetDirty(clip);
            configured++;
            Debug.Log($"[Setup] Walk_{dir}: {sprites.Count} frames configured at 12 FPS");
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Setup] Walking animations configured: {configured}/{directions.Length} directions");
    }
}
