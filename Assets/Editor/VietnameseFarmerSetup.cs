using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Complete setup tool for Vietnamese Farmer character.
/// Creates all 56 animation clips (7 actions x 8 directions) and Animator Controller.
/// 
/// Usage: Tools > Vietnamese Farmer > Complete Setup
/// </summary>
public class VietnameseFarmerSetup : EditorWindow
{
    // Character folder path
    private static readonly string CHARACTER_FOLDER = "Assets/A_Vietnamese_farmer_from_the_countryside_standing";
    
    // Output folders
    private static readonly string ANIMATIONS_FOLDER = "Assets/Animations/VietnameseFarmer";
    private static readonly string CONTROLLER_PATH = "Assets/Animations/VietnameseFarmer/VietnameseFarmerController.controller";
    
    // 7 Actions (selected from metadata)
    private static readonly string[] ACTIONS = new string[]
    {
        "breathing-idle",
        "walking-6-frames",
        "picking-up",
        "custom-A Vietnamese farmer digging soil using a farming h",
        "custom-Vietnamese farmer planting seeds by bending slight",
        "custom-Vietnamese farmer watering crops using a watering ",
        "custom-Vietnamese farmer harvesting crops by bending down"
    };
    
    // Short names for animator states
    private static readonly string[] ACTION_SHORT_NAMES = new string[]
    {
        "Idle", "Walk", "PickUp", "Dig", "Plant", "Water", "Harvest"
    };
    
    // 8 Directions
    private static readonly string[] DIRECTIONS = new string[]
    {
        "south", "south-east", "east", "north-east", "north", "north-west", "west", "south-west"
    };
    
    [MenuItem("Tools/Vietnamese Farmer/Complete Setup")]
    public static void CompleteSetup()
    {
        EditorUtility.DisplayProgressBar("Vietnamese Farmer Setup", "Starting setup...", 0);
        
        try
        {
            // Step 1: Create folders
            CreateFolders();
            
            // Step 2: Create animation clips
            CreateAllAnimationClips();
            
            // Step 3: Create Animator Controller
            CreateAnimatorController();
            
            // Step 4: Apply to player
            ApplyToPlayer();
            
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Setup Complete", 
                "Vietnamese Farmer Animator setup completed successfully!\n\n" +
                "Created 56 animation clips (7 actions x 8 directions)\n" +
                "Controller: " + CONTROLLER_PATH, "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Setup Error", "Error during setup: " + e.Message, "OK");
        }
    }
    
    private static void CreateFolders()
    {
        EditorUtility.DisplayProgressBar("Creating Folders", "Setting up folders...", 0.1f);
        
        if (!AssetDatabase.IsValidFolder(ANIMATIONS_FOLDER))
        {
            string parent = Path.GetDirectoryName(ANIMATIONS_FOLDER);
            string folderName = Path.GetFileName(ANIMATIONS_FOLDER);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
    
    private static void CreateAllAnimationClips()
    {
        int totalClips = ACTIONS.Length * DIRECTIONS.Length;
        int currentClip = 0;
        
        foreach (string action in ACTIONS)
        {
            string actionShortName = GetActionShortName(action);
            string actionFolder = Path.Combine(CHARACTER_FOLDER, "animations", action);
            
            if (!Directory.Exists(actionFolder))
            {
                Debug.LogWarning($"Action folder not found: {actionFolder}");
                continue;
            }
            
            foreach (string direction in DIRECTIONS)
            {
                currentClip++;
                EditorUtility.DisplayProgressBar(
                    "Creating Animation Clips", 
                    $"Creating {actionShortName}_{direction} ({currentClip}/{totalClips})", 
                    0.1f + (currentClip / (float)totalClips) * 0.6f);
                
                CreateAnimationClip(action, direction, actionShortName);
            }
        }
    }
    
    private static void CreateAnimationClip(string action, string direction, string actionShortName)
    {
        string directionFolder = Path.Combine(Path.Combine(CHARACTER_FOLDER, "animations", action), direction);
        
        if (!Directory.Exists(directionFolder))
        {
            Debug.LogWarning($"Direction folder not found: {directionFolder}");
            return;
        }
        
        // Get all frame files
        string[] frameFiles = Directory.GetFiles(directionFolder, "*.png");
        if (frameFiles.Length == 0)
        {
            Debug.LogWarning($"No PNG files found in {directionFolder}");
            return;
        }
        
        // Sort files
        System.Array.Sort(frameFiles);
        
        // Create clip
        AnimationClip clip = new AnimationClip();
        clip.name = $"{actionShortName}_{direction}";
        
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
            return;
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
    
    private static void CreateAnimatorController()
    {
        EditorUtility.DisplayProgressBar("Creating Animator Controller", "Setting up states and transitions...", 0.8f);
        
        // Delete existing controller if exists
        if (File.Exists(CONTROLLER_PATH))
        {
            AssetDatabase.DeleteAsset(CONTROLLER_PATH);
        }
        
        // Create controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);
        
        // Add parameters
        controller.AddParameter("Horizontal", AnimatorControllerParameterType.Float);
        controller.AddParameter("Vertical", AnimatorControllerParameterType.Float);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        
        // Add action trigger parameters
        string[] actionParams = { "PickUp", "Dig", "Plant", "Water", "Harvest" };
        foreach (string param in actionParams)
        {
            controller.AddParameter(param, AnimatorControllerParameterType.Trigger);
        }
        
        // Layer 0 - Base layer
        AnimatorControllerLayer baseLayer = controller.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;
        
        // Clear existing states
        ChildAnimatorState[] existingStates = stateMachine.states;
        foreach (ChildAnimatorState state in existingStates)
        {
            stateMachine.RemoveState(state.state);
        }
        
        // Create states for each action/direction
        var stateDict = new Dictionary<string, Dictionary<string, AnimatorState>>();
        
        for (int a = 0; a < ACTIONS.Length; a++)
        {
            string action = ACTIONS[a];
            string actionShort = ACTION_SHORT_NAMES[a];
            stateDict[action] = new Dictionary<string, AnimatorState>();
            
            foreach (string direction in DIRECTIONS)
            {
                string stateName = $"{actionShort}_{direction}";
                string clipPath = $"Assets/Animations/VietnameseFarmer/{stateName}.anim";
                
                AnimatorState state = stateMachine.AddState(stateName);
                
                // Load and assign clip
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip != null)
                {
                    state.motion = clip;
                }
                else
                {
                    Debug.LogWarning($"Clip not found: {clipPath}");
                }
                
                stateDict[action][direction] = state;
            }
        }
        
        // Set default state
        stateMachine.defaultState = stateDict["breathing-idle"]["south"];
        
        // Setup transitions
        SetupTransitions(stateMachine, stateDict);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Animator Controller created at {CONTROLLER_PATH}");
    }
    
    private static void SetupTransitions(AnimatorStateMachine stateMachine, 
        Dictionary<string, Dictionary<string, AnimatorState>> stateDict)
    {
        // Setup Idle <-> Walk transitions for each direction
        foreach (string direction in DIRECTIONS)
        {
            AnimatorState idleState = stateDict["breathing-idle"][direction];
            AnimatorState walkState = stateDict["walking-6-frames"][direction];
            
            // Idle -> Walk (when moving)
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.1f;
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0, "Speed");
            
            // Walk -> Idle (when stopped)
            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.1f;
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0, "Speed");
            
            // Direction changes for idle
            foreach (string otherDir in DIRECTIONS)
            {
                if (otherDir != direction)
                {
                    AnimatorState otherIdle = stateDict["breathing-idle"][otherDir];
                    var dirTransition = idleState.AddTransition(otherIdle);
                    dirTransition.hasExitTime = false;
                    dirTransition.duration = 0.05f;
                    // Direction change handled by Horizontal/Vertical parameters
                }
                
                if (otherDir != direction)
                {
                    AnimatorState otherWalk = stateDict["walking-6-frames"][otherDir];
                    var walkDirTransition = walkState.AddTransition(otherWalk);
                    walkDirTransition.hasExitTime = false;
                    walkDirTransition.duration = 0.05f;
                    walkDirTransition.AddCondition(AnimatorConditionMode.Greater, 0, "Speed");
                }
            }
            
            // Action transitions (PickUp, Dig, Plant, Water, Harvest)
            string[] actions = { "picking-up", "custom-A Vietnamese farmer digging soil using a farming h", 
                                 "custom-Vietnamese farmer planting seeds by bending slight",
                                 "custom-Vietnamese farmer watering crops using a watering ",
                                 "custom-Vietnamese farmer harvesting crops by bending down" };
            string[] triggers = { "PickUp", "Dig", "Plant", "Water", "Harvest" };
            
            for (int i = 0; i < actions.Length; i++)
            {
                AnimatorState actionState = stateDict[actions[i]][direction];
                
                // Idle -> Action (on trigger)
                var idleToAction = idleState.AddTransition(actionState);
                idleToAction.hasExitTime = false;
                idleToAction.duration = 0.1f;
                idleToAction.AddCondition(AnimatorConditionMode.If, 0, triggers[i]);
                
                // Walk -> Action (on trigger)
                var walkToAction = walkState.AddTransition(actionState);
                walkToAction.hasExitTime = false;
                walkToAction.duration = 0.1f;
                walkToAction.AddCondition(AnimatorConditionMode.If, 0, triggers[i]);
                
                // Action -> Idle (on exit time)
                var actionToIdle = actionState.AddTransition(idleState);
                actionToIdle.hasExitTime = true;
                actionToIdle.exitTime = 0.9f;
                actionToIdle.duration = 0.1f;
            }
        }
    }
    
    private static void ApplyToPlayer()
    {
        EditorUtility.DisplayProgressBar("Applying to Player", "Setting up player GameObject...", 0.95f);
        
        // Find player in scene
        GameObject player = GameObject.Find("Player ");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        
        if (player == null)
        {
            Debug.LogError("Player not found in scene! Please ensure there's a GameObject named 'Player' or 'Player ' in the scene.");
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
            Debug.LogError($"Animator controller not found at {CONTROLLER_PATH}");
        }
        
        // Update sprite renderer
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = player.AddComponent<SpriteRenderer>();
        }
        sr.sortingOrder = 10;
        
        // Ensure PlayerMovement script has animator reference
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null && pm.animator == null)
        {
            pm.animator = animator;
        }
        
        EditorUtility.SetDirty(player);
    }
    
    [MenuItem("Tools/Vietnamese Farmer/Rebuild Animation Clips Only")]
    public static void RebuildClips()
    {
        CreateFolders();
        CreateAllAnimationClips();
        EditorUtility.DisplayDialog("Clips Created", "Animation clips have been rebuilt.", "OK");
    }
    
    [MenuItem("Tools/Vietnamese Farmer/Rebuild Controller Only")]
    public static void RebuildController()
    {
        CreateAnimatorController();
        EditorUtility.DisplayDialog("Controller Created", "Animator Controller has been rebuilt.", "OK");
    }
}