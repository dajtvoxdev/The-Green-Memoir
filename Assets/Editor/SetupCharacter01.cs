using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tạo AnimatorController + AnimationClips cho Character01 (ElvGames Farm Game Assets).
/// Tự động:
/// 1. Tạo AnimationClips từ spritesheets (Walk Down/Up/Left/Right, Hoe Down/Up/Left/Right)
/// 2. Tạo AnimatorController với Blend Tree (params: Horizontal, Vertical, Speed)
/// 3. Gán AnimatorController và sprite mặc định lên Player
/// </summary>
public class SetupCharacter01
{
    const string CHAR_PATH = "Assets/ElvGames/Farm Game Assets/Characters/Character01/";
    const string OUTPUT_PATH = "Assets/Animations/Character01/";

    [MenuItem("Tools/SetupCharacter01")]
    public static void Setup()
    {
        // Tạo thư mục output
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder("Assets/Animations/Character01"))
            AssetDatabase.CreateFolder("Assets/Animations", "Character01");

        // === TẠO ANIMATION CLIPS ===
        // Walk animations (4 hướng, 4 frames mỗi cái)
        AnimationClip walkDown  = CreateClip("Character01_Walk_Down",  "Walk_Down",  8f);
        AnimationClip walkUp    = CreateClip("Character01_Walk_Up",    "Walk_Up",    8f);
        AnimationClip walkLeft  = CreateClip("Character01_Walk_Left",  "Walk_Left",  8f);
        AnimationClip walkRight = CreateClip("Character01_Walk_Right", "Walk_Right", 8f);

        // Idle (dùng frame đầu tiên của Walk_Down)
        AnimationClip idleDown = CreateClip("Character01_Walk_Down", "Idle_Down", 0f, true);

        // Hoe animations (cuốc đất - dùng cho farming)
        AnimationClip hoeDown  = CreateClip("Character01_Hoe_Down",  "Hoe_Down",  8f);
        AnimationClip hoeUp    = CreateClip("Character01_Hoe_Up",    "Hoe_Up",    8f);
        AnimationClip hoeLeft  = CreateClip("Character01_Hoe_Left",  "Hoe_Left",  8f);
        AnimationClip hoeRight = CreateClip("Character01_Hoe_Right", "Hoe_Right", 8f);

        if (walkDown == null || walkUp == null || walkLeft == null || walkRight == null)
        {
            Debug.LogError("Không tạo được animation clips! Kiểm tra sprites đã slice chưa.");
            return;
        }

        // === TẠO ANIMATOR CONTROLLER VỚI BLEND TREE ===
        string controllerPath = OUTPUT_PATH + "Character01_Controller.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // Thêm parameters (match PlayerMovement.cs)
        controller.AddParameter("Horizontal", AnimatorControllerParameterType.Float);
        controller.AddParameter("Vertical", AnimatorControllerParameterType.Float);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsHoeing", AnimatorControllerParameterType.Bool);

        // Lấy root state machine
        AnimatorStateMachine rootSM = controller.layers[0].stateMachine;

        // --- IDLE BLEND TREE ---
        // Idle khi Speed = 0
        AnimatorState idleState = rootSM.AddState("Idle");
        idleState.motion = idleDown;

        // --- WALK BLEND TREE ---
        // Walk khi Speed > 0, blend 4 hướng dựa trên Horizontal/Vertical
        BlendTree walkTree;
        AnimatorState walkState = controller.CreateBlendTreeInController("Walk", out walkTree);
        walkTree.blendType = BlendTreeType.SimpleDirectional2D;
        walkTree.blendParameter = "Horizontal";
        walkTree.blendParameterY = "Vertical";

        walkTree.AddChild(walkDown,  new Vector2(0, -1));   // Đi xuống
        walkTree.AddChild(walkUp,    new Vector2(0, 1));    // Đi lên
        walkTree.AddChild(walkLeft,  new Vector2(-1, 0));   // Đi trái
        walkTree.AddChild(walkRight, new Vector2(1, 0));    // Đi phải

        // --- HOE BLEND TREE ---
        BlendTree hoeTree;
        AnimatorState hoeState = controller.CreateBlendTreeInController("Hoe", out hoeTree);
        hoeTree.blendType = BlendTreeType.SimpleDirectional2D;
        hoeTree.blendParameter = "Horizontal";
        hoeTree.blendParameterY = "Vertical";

        hoeTree.AddChild(hoeDown,  new Vector2(0, -1));
        hoeTree.AddChild(hoeUp,    new Vector2(0, 1));
        hoeTree.AddChild(hoeLeft,  new Vector2(-1, 0));
        hoeTree.AddChild(hoeRight, new Vector2(1, 0));

        // --- TRANSITIONS ---
        // Idle → Walk: khi Speed > 0.01
        AnimatorStateTransition idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.01f, "Speed");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.05f;

        // Walk → Idle: khi Speed < 0.01
        AnimatorStateTransition walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.01f, "Speed");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.05f;

        // Idle → Hoe: khi IsHoeing = true
        AnimatorStateTransition idleToHoe = idleState.AddTransition(hoeState);
        idleToHoe.AddCondition(AnimatorConditionMode.If, 0, "IsHoeing");
        idleToHoe.hasExitTime = false;
        idleToHoe.duration = 0.05f;

        // Hoe → Idle: khi IsHoeing = false
        AnimatorStateTransition hoeToIdle = hoeState.AddTransition(idleState);
        hoeToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsHoeing");
        hoeToIdle.hasExitTime = true;
        hoeToIdle.duration = 0.05f;

        // Default state = Idle
        rootSM.defaultState = idleState;

        // Save
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log($"AnimatorController tạo tại: {controllerPath}");
        Debug.Log($"Animations: Walk(4), Idle(1), Hoe(4) = 9 clips");

        // === GÁN LÊN PLAYER ===
        GameObject player = GameObject.Find("Player ");
        if (player == null) player = GameObject.Find("Player");
        
        if (player != null)
        {
            // Gán Animator Controller
            Animator anim = player.GetComponent<Animator>();
            if (anim != null)
            {
                anim.runtimeAnimatorController = controller;
                EditorUtility.SetDirty(anim);
                Debug.Log("Animator Controller gán lên Player thành công!");
            }

            // Gán sprite mặc định (frame đầu Walk_Down)
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Sprite[] sprites = LoadSprites("Character01_Walk_Down");
                if (sprites != null && sprites.Length > 0)
                {
                    sr.sprite = sprites[0];
                    EditorUtility.SetDirty(sr);
                    Debug.Log("Sprite mặc định Character01 gán thành công!");
                }
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        else
        {
            Debug.LogWarning("Không tìm thấy Player trong scene! Hãy gán AnimatorController thủ công.");
        }

        Debug.Log("SetupCharacter01: HOÀN TẤT!");
    }

    /// <summary>
    /// Tạo AnimationClip từ spritesheet
    /// </summary>
    static AnimationClip CreateClip(string spritesheetName, string clipName, float fps, bool singleFrame = false)
    {
        Sprite[] sprites = LoadSprites(spritesheetName);
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"Không tìm thấy sprites cho {spritesheetName}!");
            return null;
        }

        AnimationClip clip = new AnimationClip();
        clip.name = clipName;
        clip.frameRate = fps > 0 ? fps : 12f;

        // Tạo keyframes cho sprite renderer
        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = "";
        binding.propertyName = "m_Sprite";

        int frameCount = singleFrame ? 1 : sprites.Length;
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[frameCount];

        for (int i = 0; i < frameCount; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe();
            keyframes[i].time = i / clip.frameRate;
            keyframes[i].value = sprites[i];
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Loop setting (walk = loop, idle = loop, hoe = không loop)
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = !clipName.Contains("Hoe"); // Walk và Idle loop, Hoe không
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Save clip
        string clipPath = OUTPUT_PATH + clipName + ".anim";
        AssetDatabase.CreateAsset(clip, clipPath);
        Debug.Log($"Animation clip: {clipName} ({sprites.Length} frames) → {clipPath}");

        return clip;
    }

    /// <summary>
    /// Load tất cả sub-sprites từ spritesheet
    /// </summary>
    static Sprite[] LoadSprites(string spritesheetName)
    {
        string path = CHAR_PATH + spritesheetName + ".png";
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        
        List<Sprite> sprites = new List<Sprite>();
        foreach (Object obj in assets)
        {
            if (obj is Sprite s)
                sprites.Add(s);
        }

        // Sắp xếp theo tên (để đúng thứ tự frame)
        sprites.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        
        return sprites.ToArray();
    }
}
