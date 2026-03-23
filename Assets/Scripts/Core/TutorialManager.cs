using System;
using UnityEngine;

/// <summary>
/// Tracks first-time player tutorial steps and shows a checklist overlay.
/// Steps are persisted to PlayerPrefs so the tutorial only runs once.
///
/// Phase 3 Feature (#35): Tutorial/Onboarding.
///
/// Steps:
///   Move   → WalkAround  (detected via player position change)
///   Till   → TillSoil    (called by PlayerFarmController.HandleTill)
///   Plant  → PlantSeed   (called by PlayerFarmController.PlantCropAtPlayer)
///   Water  → WaterCrop   (called by PlayerFarmController.HandleWater)
///   Harvest→ HarvestCrop (called by PlayerFarmController.HandleHarvest)
///
/// Usage: TutorialManager.Instance?.CompleteStep(TutorialStep.TillSoil);
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public enum TutorialStep { WalkAround = 0, TillSoil = 1, PlantSeed = 2, WaterCrop = 3, HarvestCrop = 4 }

    private const string PREFS_PREFIX = "tutorial_step_";
    private const string PREFS_DONE   = "tutorial_complete";

    private const int STEP_COUNT = 5;

    private bool[] completed = new bool[STEP_COUNT];
    private bool tutorialDone = false;

    /// <summary>Fired when any step is completed. Arg: the step.</summary>
    public event Action<TutorialStep> OnStepCompleted;

    /// <summary>Fired when all steps are done.</summary>
    public event Action OnTutorialComplete;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        LoadProgress();

        if (tutorialDone)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void Update()
    {
        if (tutorialDone) return;

        // Auto-detect movement for WalkAround step
        if (!completed[(int)TutorialStep.WalkAround])
        {
            DetectMovement();
        }
    }

    // ==================== PUBLIC API ====================

    /// <summary>
    /// Call this from game systems when a tutorial action is performed.
    /// Safe to call even after tutorial is complete (no-ops).
    /// </summary>
    public void CompleteStep(TutorialStep step)
    {
        if (tutorialDone) return;
        if (completed[(int)step]) return;

        completed[(int)step] = true;
        PlayerPrefs.SetInt(PREFS_PREFIX + (int)step, 1);

        OnStepCompleted?.Invoke(step);
        ShowStepCompletionHint(step);

        CheckAllComplete();
    }

    public bool IsStepDone(TutorialStep step) => completed[(int)step];
    public bool IsTutorialDone()              => tutorialDone;

    // ==================== PRIVATE ====================

    private Vector3 _lastPlayerPos;
    private bool    _posInitialized = false;

    private void DetectMovement()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        if (!_posInitialized)
        {
            _lastPlayerPos = player.transform.position;
            _posInitialized = true;
            return;
        }

        if (Vector3.Distance(player.transform.position, _lastPlayerPos) > 0.5f)
        {
            CompleteStep(TutorialStep.WalkAround);
        }
    }

    private void ShowStepCompletionHint(TutorialStep step)
    {
        string msg = step switch
        {
            TutorialStep.WalkAround  => "Di chuyển thành công! Dùng WASD hoặc click chuột để đi.",
            TutorialStep.TillSoil    => "Cuốc đất xong! Chọn hạt (1-9), rồi chuột phải để gieo.",
            TutorialStep.PlantSeed   => "Gieo hạt xong! Chuột phải vào cây để tưới nước.",
            TutorialStep.WaterCrop   => "Tưới nước xong! Cây sẽ lớn theo thời gian.",
            TutorialStep.HarvestCrop => "Thu hoạch thành công! Bạn đã hoàn thành hướng dẫn!",
            _ => ""
        };

        if (!string.IsNullOrEmpty(msg))
        {
            NotificationManager.Instance?.ShowNotification(msg, 3f);
        }
    }

    private void CheckAllComplete()
    {
        for (int i = 0; i < STEP_COUNT; i++)
        {
            if (!completed[i]) return;
        }

        tutorialDone = true;
        PlayerPrefs.SetInt(PREFS_DONE, 1);
        OnTutorialComplete?.Invoke();
        NotificationManager.Instance?.ShowNotification(
            "Hướng dẫn hoàn thành! Chúc bạn chơi vui vẻ 🌾", 4f);
        Debug.Log("TutorialManager: Tutorial complete!");
    }

    private void LoadProgress()
    {
        tutorialDone = PlayerPrefs.GetInt(PREFS_DONE, 0) == 1;
        if (tutorialDone) return;

        for (int i = 0; i < STEP_COUNT; i++)
        {
            completed[i] = PlayerPrefs.GetInt(PREFS_PREFIX + i, 0) == 1;
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
