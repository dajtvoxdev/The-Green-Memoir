using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Editor tool to build Animator Controller for Vietnamese Farmer character
/// with 7 actions and 8 directions (56 animation clips total)
/// </summary>
public class VietnameseFarmerAnimatorBuilder : EditorWindow
{
    // Character folder path
    private static readonly string CHARACTER_FOLDER = "Assets/A_Vietnamese_farmer_from_the_countryside_standing";
    
    // Output folders
    private static readonly string ANIMATIONS_FOLDER = "Assets/Animations/VietnameseFarmer";
    private static readonly string CONTROLLER_PATH = "Assets/Animations/VietnameseFarmer/VietnameseFarmerController.controller";
    
    // 7 Actions (selected from metadata)
    private static readonly string[] ACTIONS = new string[]
    {
        "breathing-idle",      // Idle
        "walking-6-frames",    // Walk
        "picking-up",          // PickUp
        "custom-A Vietnamese farmer digging soil using a farming h", // Dig
        "custom-Vietnamese farmer planting seeds by bending slight", // Plant
        "custom-Vietnamese farmer watering crops using a watering ", // Water
        "custom-Vietnamese farmer harvesting crops by bending down"  // Harvest
    };
    
    // Animator parameter names for each action
    private static readonly string[] ACTION_PARAMS = new string[]
    {
        "Idle", "Walk", "PickUp", "Dig", "Plant", "Water", "Harvest"
    };
    
    // 8 Directions
    private static readonly string[] DIRECTIONS = new string[]
    {
        "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west"
    };
    
    // Direction vectors for each direction (for animator)
    private static readonly Vector2[] DIRECTION_VECTORS = new Vector2[]
    {
        new Vector2(0, -1),   // south
        new Vector2(1, -1),   // south-east
        new Vector2(1, 0),    // east
        new Vector2(1, 1),    // north-east
        new Vector2(0, 1),    // north
        new Vector2(-1, 1),   // north-west
        new Vector2(-1, 0),   // west
        new Vector2(-1, -1)   // south-west
    };
    
    [MenuItem("Tools/Vietnamese Farmer/Build Animator")]
    public static void BuildAnimator()
    {
        // Create animations folder
        if (!AssetDatabase.IsValidFolder(ANIMATIONS_FOLDER))
        {
            string parent = Path.GetDirectoryName(ANIMATIONS_FOLDER);
            string folderName = Path.GetFileName(ANIMATIONS_FOLDER);
            AssetDatabase.CreateFolder(parent, folderName);
        }
        
        // Create controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
        
        // Add parameters
        controller.AddParameter("Horizontal", AnimatorControllerParameterType.Float);
        controller.AddParameter("Vertical", AnimatorControllerParameterType.Float);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        
        // Add action trigger parameters
        foreach (string param in ACTION_PARAMS)
        {
            if (param != "Idle" && param != "Walk")
            {
                controller.AddParameter(param, AnimatorControllerParameterType.Trigger);
            }
        }
        
        // Layer 0 - Base layer
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine baseStateMachine = baseLayer.stateMachine;
        
        // Clear existing states
        ChildAnimatorState[] existingStates = baseStateMachine.states;
        foreach (ChildAnimatorState state in existingStates)
        {
            baseStateMachine.RemoveState(state.state);
        }
        
        // Create dictionary to store states for each action/direction
        Dictionary<string, Dictionary<string, AnimatorState>> stateDict = 
            new Dictionary<string, Dictionary<string, AnimatorState>>();
        
        // Create states for each action/direction combination
        foreach (string action in ACTIONS)
        {
            stateDict[action] = new Dictionary<string, AnimatorState>();
            
            foreach (string direction in DIRECTIONS)
            {
                string stateName = $"{GetActionShortName(action)}_{direction}";
                AnimatorState state = baseStateMachine.AddState(stateName);
                
                // Create animation clip
                AnimationClip clip = CreateAnimationClip(action, direction);
                if (clip != null)
                {
                    state.motion = clip;
                }
                
                stateDict[action][direction] = state;
            }
        }
        
        // Set up transitions
        SetupTransitions(baseStateMachine, stateDict);
        
        // Save
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Vietnamese Farmer Animator built successfully at {CONTROLLER_PATH}");
        Debug.Log($"Total animation clips created: {ACTIONS.Length * DIRECTIONS.Length}");
    }
    
    private static string GetActionShortName(string action)
    {
        switch (action)
        {
            case "breathing-idle": return "Idle";
            case "walking-6-frames": return "Walk";
            case "picking-up": return "PickUp";
            case "custom-A Vietnamese farmer digging soil using a farming h": return "Dig";
            case "custom-Vietnamese farmer planting seeds by bending slight": return "Plant";
            case "custom-Vietnamese farmer watering crops using a watering ": return "Water";
            case "custom-Vietnamese farmer harvesting crops by bending down": return "Harvest";
            default: return action.Replace("custom-", "").Replace(" ", "_");
        }
    }
    
    private static AnimationClip CreateAnimationClip(string action, string direction)
    {
        string actionFolder = Path.Combine(CHARACTER_FOLDER, "animations", action);
        string directionFolder = Path.Combine(actionFolder, direction);
        
        if (!Directory.Exists(directionFolder))
        {
            Debug.LogWarning($"Directory not found: {directionFolder}");
            return null;
        }
        
        // Get all frame files
        string[] frameFiles = Directory.GetFiles(directionFolder, "*.png");
        if (frameFiles.Length == 0)
        {
            Debug.LogWarning($"No PNG files found in {directionFolder}");
            return null;
        }
        
        // Sort files
        System.Array.Sort(frameFiles);
        
        // Create clip
        AnimationClip clip = new AnimationClip();
        clip.name = $"{GetActionShortName(action)}_{direction}";
        
        // Load sprites
        List<Sprite> sprites = new List<Sprite>();
        foreach (string file in frameFiles)
        {
            string assetPath = file.Replace("\\", "/");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }
        
        if (sprites.Count == 0)
        {
            Debug.LogWarning($"No valid sprites loaded for {clip.name}");
            return null;
        }
        
        // Create animation curve for sprite switching
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        float frameRate = 12f; // 12 FPS
        float frameDuration = 1f / frameRate;
        
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameDuration,
                value = sprites[i]
            };
        }
        
        // Set curve for m_Sprite property
        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        
        // Set clip settings
        clip.frameRate = frameRate;
        
        // Save clip
        string clipPath = Path.Combine(ANIMATIONS_FOLDER, $"{clip.name}.anim").Replace("\\", "/");
        AssetDatabase.CreateAsset(clip, clipPath);
        
        Debug.Log($"Created animation clip: {clipPath} ({sprites.Count} frames)");
        
        return clip;
    }
    
    private static void SetupTransitions(AnimatorStateMachine stateMachine, 
        Dictionary<string, Dictionary<string, AnimatorState>> stateDict)
    {
        // Get idle south as default state
        AnimatorState defaultState = stateDict["breathing-idle"]["south"];
        stateMachine.defaultState = defaultState;
        
        // Create transitions between states based on direction and action
        foreach (string action in ACTIONS)
        {
            foreach (string direction in DIRECTIONS)
            {
                AnimatorState currentState = stateDict[action][direction];
                
                // For idle states - transition based on direction change
                if (action == "breathing-idle")
                {
                    foreach (string otherDir in DIRECTIONS)
                    {
                        if (otherDir != direction)
                        {
                            AnimatorState otherState = stateDict[action][otherDir];
                            CreateTransition(currentState, otherState, stateMachine, 
                                $"Horizontal < {GetDirectionThreshold(direction, otherDir, true)} && Horizontal > {GetDirectionThreshold(direction, otherDir, false)}");
                        }
                    }
                    
                    // Transition to walk when moving
                    AnimatorState walkState = stateDict["walking-6-frames"][direction];
                    CreateTransition(currentState, walkState, stateMachine, "Speed > 0.1");
                }
                
                // For walk states - transition based on direction change and stop
                if (action == "walking-6-frames")
                {
                    foreach (string otherDir in DIRECTIONS)
                    {
                        if (otherDir != direction)
                        {
                            AnimatorState otherState = stateDict[action][otherDir];
                            CreateTransition(currentState, otherState, stateMachine, "Speed > 0.1");
                        }
                    }
                    
                    // Transition to idle when stopped
                    AnimatorState idleState = stateDict["breathing-idle"][direction];
                    CreateTransition(currentState, idleState, stateMachine, "Speed < 0.1");
                }
                
                // For action states - transition back to idle when complete
                if (action != "breathing-idle" && action != "walking-6-frames")
                {
                    AnimatorState idleState = stateDict["breathing-idle"][direction];
                    // Exit time transition (when animation completes)
                    var transition = currentState.AddTransition(idleState);
                    transition.hasExitTime = true;
                    transition.exitTime = 0.9f; // Transition at 90% of animation
                    transition.duration = 0.1f;
                }
            }
        }
    }
    
    private static float GetDirectionThreshold(string fromDir, string toDir, bool isUpper)
    {
        int fromIndex = System.Array.IndexOf(DIRECTIONS, fromDir);
        int toIndex = System.Array.IndexOf(DIRECTIONS, toDir);
        
        // Simple threshold based on direction indices
        float threshold = (fromIndex + toIndex) / 2f - 3.5f;
        return isUpper ? threshold + 0.5f : threshold - 0.5f;
    }
    
    private static void CreateTransition(AnimatorState fromState, AnimatorState toState, 
        AnimatorStateMachine stateMachine, string condition)
    {
        var transition = fromState.AddTransition(toState);
        transition.hasExitTime = false;
        transition.duration = 0.1f;
        
        // Parse condition and add
        if (condition.Contains("Speed >"))
        {
            transition.AddCondition(AnimatorConditionMode.Greater, 0, "Speed");
        }
        else if (condition.Contains("Speed <"))
        {
            transition.AddCondition(AnimatorConditionMode.Less, 0, "Speed");
        }
        // Add more condition parsing as needed
    }
    
    [MenuItem("Tools/Vietnamese Farmer/Apply to Player")]
    public static void ApplyToPlayer()
    {
        // Find player in scene
        GameObject player = GameObject.Find("Player ");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        if (player == null)
        {
            Debug.LogError("Player not found in scene!");
            return;
        }
        
        // Apply animator controller
        Animator animator = player.GetComponent<Animator>();
        if (animator == null)
        {
            animator = player.AddComponent<Animator>();
        }
        
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER_PATH);
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log($"Applied Vietnamese Farmer Animator to {player.name}");
        }
        else
        {
            Debug.LogError($"Animator controller not found at {CONTROLLER_PATH}. Please build animator first.");
        }
        
        // Update sprite renderer sorting order for proper layering
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = player.AddComponent<SpriteRenderer>();
        }
        sr.sortingOrder = 10;
        
        EditorUtility.SetDirty(player);
    }
}