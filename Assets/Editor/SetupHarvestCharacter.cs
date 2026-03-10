using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Editor tool to setup the HarvestQuestFree character:
/// - Imports and slices CharacterFree.png into animation frames
/// - Creates walk/idle animation clips for 4 directions
/// - Builds an Animator Controller with Blend Trees
/// - Assigns everything to the Player GameObject
///
/// Compatible with existing PlayerMovement.cs (Horizontal, Vertical, Speed parameters).
///
/// Run via menu: Tools > Moonlit Garden > Setup Harvest Character
/// </summary>
public class SetupHarvestCharacter : EditorWindow
{
    private const string CHAR_PATH = "Assets/HarvestQuestFree/CharacterFree.png";
    private const string ANIM_FOLDER = "Assets/Animations";
    private const string CONTROLLER_PATH = "Assets/Animations/HQ_Player.controller";

    // CharacterFree.png: 384x576, grid layout analysis:
    // Each frame appears to be ~96x64 (4 cols x 9 rows)
    // But on closer inspection, the sprites are small characters (~24x32 actual pixels)
    // centered in larger cells. Let's use manual cell size based on the grid pattern.
    //
    // Row 0 (top): Walk Down - 4 frames
    // Row 1: Walk Up - 4 frames (back view)
    // Row 2: Walk Left - 4 frames
    // Row 3: Walk Down variant - 5 frames (with arms moving more)
    // Row 4: Walk Up variant - 5 frames
    // Row 5: Walk Left/Side variant - 5 frames
    // Row 6: Swing/Attack Down - 4 frames (with tool)
    // Row 7: Swing/Attack variant - 4 frames
    // Row 8: Swing/Attack variant - 4 frames (bottom row, partial)

    private const int CELL_W = 96;  // 384 / 4 = 96
    private const int CELL_H = 64;  // 576 / 9 = 64
    private const int COLS = 4;
    private const int ROWS = 9;
    private const int PPU = 32;     // Makes character ~3 tiles tall (64/32=2 units) - good size
    private const int ANIM_FPS = 8;

    [MenuItem("Tools/Moonlit Garden/Setup Harvest Character")]
    public static void SetupCharacter()
    {
        Debug.Log("SetupHarvestCharacter: Starting character setup...");

        // Step 1: Import & slice the character sprite sheet
        var sprites = ImportAndSlice();
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("SetupHarvestCharacter: No sprites found after import!");
            return;
        }
        Debug.Log($"SetupHarvestCharacter: {sprites.Length} sprites sliced");

        // Step 2: Create animation clips
        if (!AssetDatabase.IsValidFolder(ANIM_FOLDER))
            AssetDatabase.CreateFolder("Assets", "Animations");

        var clips = CreateAnimationClips(sprites);
        Debug.Log($"SetupHarvestCharacter: {clips.Count} animation clips created");

        // Step 3: Build Animator Controller
        var controller = BuildAnimatorController(clips);
        Debug.Log("SetupHarvestCharacter: Animator Controller built");

        // Step 4: Assign to Player in scene
        AssignToPlayer(sprites, controller);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("SetupHarvestCharacter: Character setup complete!");
    }

    // ─── IMPORT & SLICE ─────────────────────────────────────────────

    static Sprite[] ImportAndSlice()
    {
        TextureImporter imp = AssetImporter.GetAtPath(CHAR_PATH) as TextureImporter;
        if (imp == null)
        {
            Debug.LogError("SetupHarvestCharacter: Cannot find CharacterFree.png!");
            return null;
        }

        bool changed = false;

        if (imp.spritePixelsPerUnit != PPU) { imp.spritePixelsPerUnit = PPU; changed = true; }
        if (imp.filterMode != FilterMode.Point) { imp.filterMode = FilterMode.Point; changed = true; }
        if (imp.textureCompression != TextureImporterCompression.Uncompressed)
        { imp.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (imp.textureType != TextureImporterType.Sprite)
        { imp.textureType = TextureImporterType.Sprite; changed = true; }

        // Slice into grid
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(CHAR_PATH);
        if (tex == null) return null;

        int texW = tex.width, texH = tex.height;
        int cols = texW / CELL_W;
        int rows = texH / CELL_H;

        if (imp.spriteImportMode != SpriteImportMode.Multiple || changed)
        {
            imp.spriteImportMode = SpriteImportMode.Multiple;

            var metaData = new List<SpriteMetaData>();
            int idx = 0;
            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    metaData.Add(new SpriteMetaData
                    {
                        name = $"CharFree_{idx}",
                        rect = new Rect(col * CELL_W, row * CELL_H, CELL_W, CELL_H),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                    idx++;
                }
            }

#pragma warning disable CS0618
            imp.spritesheet = metaData.ToArray();
#pragma warning restore CS0618
            imp.SaveAndReimport();
            Debug.Log($"SetupHarvestCharacter: Sliced {metaData.Count} frames ({cols}x{rows})");
        }

        // Load all sprites with proper numeric sorting
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(CHAR_PATH);
        return allAssets.OfType<Sprite>()
            .OrderBy(s => {
                // Extract numeric index from "CharFree_N"
                string numPart = s.name.Replace("CharFree_", "");
                return int.TryParse(numPart, out int idx) ? idx : 999;
            })
            .ToArray();
    }

    // ─── CREATE ANIMATION CLIPS ─────────────────────────────────────

    static Dictionary<string, AnimationClip> CreateAnimationClips(Sprite[] sprites)
    {
        var clips = new Dictionary<string, AnimationClip>();

        // Slicing loop: row=rows-1 (top, y=512) first → idx 0, down to row=0 (bottom, y=0) → idx 32
        // In Unity texture coords, higher Y = higher on texture = TOP visually
        //
        // idx 0-3:   row=8, y=512 → TOP of texture → Walk Down (front facing, no weapon)
        // idx 4-7:   row=7, y=448 → Walk Up (back view)
        // idx 8-11:  row=6, y=384 → Walk Left (side view)
        // idx 12-15: row=5, y=320 → Walk Down variant (extended)
        // idx 16-19: row=4, y=256 → Walk Up variant (extended)
        // idx 20-23: row=3, y=192 → Walk Left variant (extended)
        // idx 24-27: row=2, y=128 → Swing/Attack Down (with tool)
        // idx 28-31: row=1, y=64  → Swing/Attack variant
        // idx 32-35: row=0, y=0   → BOTTOM → Swing/Attack variant (sword!)

        int walkDownStart  = 0;  // TOP row: walk down / front facing (no weapon)
        int walkUpStart    = 4;  // 2nd row: walk up / back facing
        int walkLeftStart  = 8;  // 3rd row: walk left / side

        // Walk animations (4 frames each, looping)
        clips["HQ_WalkDown"]  = CreateClip("HQ_WalkDown",  sprites, walkDownStart, 4, true);
        clips["HQ_WalkUp"]    = CreateClip("HQ_WalkUp",    sprites, walkUpStart, 4, true);
        clips["HQ_WalkLeft"]  = CreateClip("HQ_WalkLeft",  sprites, walkLeftStart, 4, true);
        clips["HQ_WalkRight"] = CreateClip("HQ_WalkRight", sprites, walkLeftStart, 4, true);

        // Idle (single frame)
        clips["HQ_IdleDown"]  = CreateClip("HQ_IdleDown",  sprites, walkDownStart, 1, false);
        clips["HQ_IdleUp"]    = CreateClip("HQ_IdleUp",    sprites, walkUpStart, 1, false);
        clips["HQ_IdleLeft"]  = CreateClip("HQ_IdleLeft",  sprites, walkLeftStart, 1, false);
        clips["HQ_IdleRight"] = CreateClip("HQ_IdleRight", sprites, walkLeftStart, 1, false);

        return clips;
    }

    static AnimationClip CreateClip(string name, Sprite[] allSprites, int startIdx, int frameCount, bool loop)
    {
        string clipPath = $"{ANIM_FOLDER}/{name}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);

        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.frameRate = ANIM_FPS;

        // Build keyframes for SpriteRenderer.m_Sprite
        var keyframes = new List<ObjectReferenceKeyframe>();
        for (int i = 0; i < frameCount && (startIdx + i) < allSprites.Length; i++)
        {
            keyframes.Add(new ObjectReferenceKeyframe
            {
                time = i / (float)ANIM_FPS,
                value = allSprites[startIdx + i]
            });
        }

        // Add end keyframe to set clip length
        if (keyframes.Count > 1)
        {
            keyframes.Add(new ObjectReferenceKeyframe
            {
                time = frameCount / (float)ANIM_FPS,
                value = allSprites[startIdx] // loop back to first frame
            });
        }

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes.ToArray());

        // Set loop
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        EditorUtility.SetDirty(clip);

        return clip;
    }

    // ─── BUILD ANIMATOR CONTROLLER ──────────────────────────────────

    static AnimatorController BuildAnimatorController(Dictionary<string, AnimationClip> clips)
    {
        // Delete existing if present
        if (File.Exists(CONTROLLER_PATH))
            AssetDatabase.DeleteAsset(CONTROLLER_PATH);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);

        // Add parameters (matching PlayerMovement.cs)
        controller.AddParameter("Horizontal", AnimatorControllerParameterType.Float);
        controller.AddParameter("Vertical", AnimatorControllerParameterType.Float);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        // Get base layer
        var rootStateMachine = controller.layers[0].stateMachine;

        // ═══ IDLE BLEND TREE ═══
        var idleState = controller.CreateBlendTreeInController("Idle", out BlendTree idleTree, 0);
        idleTree.blendType = BlendTreeType.SimpleDirectional2D;
        idleTree.blendParameter = "Horizontal";
        idleTree.blendParameterY = "Vertical";

        if (clips.ContainsKey("HQ_IdleDown"))
            idleTree.AddChild(clips["HQ_IdleDown"], new Vector2(0, -1));
        if (clips.ContainsKey("HQ_IdleUp"))
            idleTree.AddChild(clips["HQ_IdleUp"], new Vector2(0, 1));
        if (clips.ContainsKey("HQ_IdleLeft"))
            idleTree.AddChild(clips["HQ_IdleLeft"], new Vector2(-1, 0));
        if (clips.ContainsKey("HQ_IdleRight"))
            idleTree.AddChild(clips["HQ_IdleRight"], new Vector2(1, 0));

        // ═══ WALK BLEND TREE ═══
        var walkState = controller.CreateBlendTreeInController("Walk", out BlendTree walkTree, 0);
        walkTree.blendType = BlendTreeType.SimpleDirectional2D;
        walkTree.blendParameter = "Horizontal";
        walkTree.blendParameterY = "Vertical";

        if (clips.ContainsKey("HQ_WalkDown"))
            walkTree.AddChild(clips["HQ_WalkDown"], new Vector2(0, -1));
        if (clips.ContainsKey("HQ_WalkUp"))
            walkTree.AddChild(clips["HQ_WalkUp"], new Vector2(0, 1));
        if (clips.ContainsKey("HQ_WalkLeft"))
            walkTree.AddChild(clips["HQ_WalkLeft"], new Vector2(-1, 0));
        if (clips.ContainsKey("HQ_WalkRight"))
            walkTree.AddChild(clips["HQ_WalkRight"], new Vector2(1, 0));

        // Set Idle as default state
        rootStateMachine.defaultState = idleState;

        // ═══ TRANSITIONS ═══
        // Idle → Walk (when Speed > 0.01)
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;

        // Walk → Idle (when Speed < 0.01)
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        return controller;
    }

    // ─── ASSIGN TO PLAYER ───────────────────────────────────────────

    static void AssignToPlayer(Sprite[] sprites, AnimatorController controller)
    {
        // Find Player in scene (note: name has trailing space "Player ")
        GameObject player = null;
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.Trim() == "Player")
            {
                player = go;
                break;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("SetupHarvestCharacter: Player not found in scene!");
            return;
        }

        // Set sprite to idle-down frame (idx 0 = top row = walk down, no weapon)
        var sr = player.GetComponent<SpriteRenderer>();
        if (sr != null && sprites.Length > 0)
        {
            sr.sprite = sprites[0]; // Walk-down first frame = idle facing front (no weapon)
            EditorUtility.SetDirty(sr);
            Debug.Log($"SetupHarvestCharacter: Player sprite set to {sprites[0].name}");
        }

        // Set animator controller
        var animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            var so = new SerializedObject(animator);
            var controllerProp = so.FindProperty("m_Controller");
            controllerProp.objectReferenceValue = controller;
            so.ApplyModifiedProperties();
            Debug.Log("SetupHarvestCharacter: Animator controller assigned");
        }
    }
}
