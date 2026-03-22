using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Linq;

/// <summary>
/// Editor utility that sets up the farming system:
/// 1. Assigns tilled soil tile to PlayerFarmController
/// 2. Creates farming animation clips from PixelLab sprite frames
/// 3. Adds farming animation states to the Animator controller
///
/// Run via: Tools > Setup Farming System
/// </summary>
public class SetupFarmingSystem : Editor
{
    private const string ANIM_BASE = "Assets/A_Vietnamese_farmer_from_the_countryside_standing/animations";
    private const string ANIM_OUTPUT = "Assets/Animations/VietnameseFarmer";

    // Map custom animation folder names to action trigger names
    private static readonly (string folder, string trigger)[] AnimMappings = new[]
    {
        ("custom-A Vietnamese farmer digging soil using a farming h", "Dig"),
        ("custom-Vietnamese farmer planting seeds by bending slight", "Plant"),
        ("custom-Vietnamese farmer watering crops using a watering", "Water"),
        ("custom-Vietnamese farmer harvesting crops by bending down", "Harvest"),
    };

    private static readonly string[] Directions = new[]
    {
        "south", "north", "east", "west",
        "south-east", "south-west", "north-east", "north-west"
    };

    [MenuItem("Tools/Setup Farming System")]
    public static void Setup()
    {
        AssignTilledSoilTile();
        CreateFarmingAnimations();
        AddFarmingStatesToAnimator();
        AssetDatabase.SaveAssets();
        Debug.Log("[SetupFarmingSystem] Done! Save the scene to persist changes.");
    }

    /// <summary>
    /// Assigns tileset_farmsoil_6 (pure tilled soil) to PlayerFarmController.tb_TilledSoil.
    /// </summary>
    private static void AssignTilledSoilTile()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("[SetupFarmingSystem] Player not found in scene!");
            return;
        }

        var farmController = player.GetComponent<PlayerFarmController>();
        if (farmController == null)
        {
            Debug.LogError("[SetupFarmingSystem] PlayerFarmController not found on Player!");
            return;
        }

        var tile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Sprites/Tilesets/tileset_farmsoil_6.asset");
        if (tile == null)
        {
            Debug.LogWarning("[SetupFarmingSystem] tileset_farmsoil_6.asset not found, skipping tile assignment.");
            return;
        }

        farmController.tb_TilledSoil = tile;
        EditorUtility.SetDirty(farmController);
        Debug.Log($"[SetupFarmingSystem] Assigned tb_TilledSoil = {tile.name}");
    }

    /// <summary>
    /// Creates AnimationClip assets from PixelLab sprite frame folders.
    /// Each action (Dig, Plant, Water, Harvest) × each direction = one clip.
    /// </summary>
    private static void CreateFarmingAnimations()
    {
        if (!Directory.Exists(ANIM_OUTPUT))
            Directory.CreateDirectory(ANIM_OUTPUT);

        int createdCount = 0;

        foreach (var (folder, trigger) in AnimMappings)
        {
            string folderPath = $"{ANIM_BASE}/{folder}";
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[SetupFarmingSystem] Animation folder not found: {folderPath}");
                continue;
            }

            Debug.Log($"[SetupFarmingSystem] Processing action: {trigger} from {folderPath}");

            foreach (string dir in Directions)
            {
                string dirPath = $"{folderPath}/{dir}";
                if (!Directory.Exists(dirPath))
                {
                    Debug.Log($"[SetupFarmingSystem] Direction folder missing: {dirPath}");
                    continue;
                }

                string clipName = $"{trigger}_{dir}";
                string clipPath = $"{ANIM_OUTPUT}/{clipName}.anim";

                // Skip if already exists
                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
                {
                    Debug.Log($"[SetupFarmingSystem] Clip already exists: {clipName}");
                    continue;
                }

                // Find all frame PNGs
                var framePaths = Directory.GetFiles(dirPath, "frame_*.png")
                    .OrderBy(f => f)
                    .ToArray();

                Debug.Log($"[SetupFarmingSystem] {clipName}: found {framePaths.Length} frames");
                if (framePaths.Length == 0) continue;

                // Load sprites — for Multiple sprite mode, load all sub-sprites and pick first
                var sprites = new Sprite[framePaths.Length];
                for (int i = 0; i < framePaths.Length; i++)
                {
                    string assetPath = framePaths[i].Replace("\\", "/");
                    // Make path relative to project
                    int assetsIdx = assetPath.IndexOf("Assets/");
                    if (assetsIdx >= 0) assetPath = assetPath.Substring(assetsIdx);

                    // Try direct load first
                    sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

                    // If null, try loading all sub-assets (Multiple sprite mode)
                    if (sprites[i] == null)
                    {
                        var subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                        foreach (var sub in subAssets)
                        {
                            if (sub is Sprite s)
                            {
                                sprites[i] = s;
                                break;
                            }
                        }
                    }

                    if (sprites[i] == null)
                        Debug.LogWarning($"[SetupFarmingSystem] Failed to load sprite: {assetPath}");
                }

                if (sprites.Any(s => s == null))
                {
                    Debug.LogWarning($"[SetupFarmingSystem] Some sprites null for {clipName}, skipping");
                    continue;
                }

                // Create animation clip
                var clip = new AnimationClip();
                clip.frameRate = 8; // 8 fps for farming animations

                var binding = new EditorCurveBinding
                {
                    type = typeof(SpriteRenderer),
                    path = "",
                    propertyName = "m_Sprite"
                };

                var keyframes = new ObjectReferenceKeyframe[sprites.Length + 1];
                float frameDuration = 1f / clip.frameRate;

                for (int i = 0; i < sprites.Length; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe
                    {
                        time = i * frameDuration,
                        value = sprites[i]
                    };
                }
                // Hold last frame
                keyframes[sprites.Length] = new ObjectReferenceKeyframe
                {
                    time = sprites.Length * frameDuration,
                    value = sprites[sprites.Length - 1]
                };

                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                // Non-looping (one-shot action)
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                AssetDatabase.CreateAsset(clip, clipPath);
                createdCount++;
            }
        }

        Debug.Log($"[SetupFarmingSystem] Created {createdCount} farming animation clips");
    }

    /// <summary>
    /// Adds farming action states (Dig, Plant, Water, Harvest) to the VietnameseFarmer
    /// Animator controller with trigger parameters.
    /// </summary>
    private static void AddFarmingStatesToAnimator()
    {
        string controllerPath = "Assets/Animations/VietnameseFarmer/VietnameseFarmerController.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogWarning($"[SetupFarmingSystem] Animator controller not found at {controllerPath}");
            return;
        }

        // Add trigger parameters if not already present
        string[] triggers = { "Dig", "Plant", "Water", "Harvest" };
        foreach (string trigger in triggers)
        {
            bool exists = controller.parameters.Any(p => p.name == trigger);
            if (!exists)
            {
                controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);
                Debug.Log($"[SetupFarmingSystem] Added trigger parameter: {trigger}");
            }
        }

        // Get the base layer
        var baseLayer = controller.layers[0];
        var stateMachine = baseLayer.stateMachine;

        int statesAdded = 0;

        foreach (string trigger in triggers)
        {
            foreach (string dir in Directions)
            {
                string clipName = $"{trigger}_{dir}";
                string clipPath = $"{ANIM_OUTPUT}/{clipName}.anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null) continue;

                // Check if state already exists
                bool stateExists = stateMachine.states.Any(s => s.state.name == clipName);
                if (stateExists) continue;

                // Add state
                var state = stateMachine.AddState(clipName);
                state.motion = clip;

                // Find corresponding Idle state to transition back to
                string idleName = $"Idle_{dir}";
                var idleState = stateMachine.states
                    .FirstOrDefault(s => s.state.name == idleName).state;

                if (idleState != null)
                {
                    // Transition from Any State → action (on trigger)
                    var transition = stateMachine.AddAnyStateTransition(state);
                    transition.AddCondition(AnimatorConditionMode.If, 0, trigger);
                    transition.duration = 0.05f;
                    transition.hasExitTime = false;

                    // Transition from action → Idle (on exit)
                    var exitTransition = state.AddTransition(idleState);
                    exitTransition.hasExitTime = true;
                    exitTransition.exitTime = 1f;
                    exitTransition.duration = 0.1f;
                }

                statesAdded++;
            }
        }

        EditorUtility.SetDirty(controller);
        Debug.Log($"[SetupFarmingSystem] Added {statesAdded} animation states to Animator");
    }
}
