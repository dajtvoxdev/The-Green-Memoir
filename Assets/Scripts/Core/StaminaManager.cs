using System;
using UnityEngine;

/// <summary>
/// Manages player stamina, a finite daily resource consumed by farm actions.
/// Fully restores at the start of each new in-game day.
///
/// Phase 3 Feature (#33): Energy / Stamina System.
///
/// Action costs:
///   Till = 1, Plant = 2, Water = 1, Harvest = 2
///
/// Usage:
///   bool ok = StaminaManager.Instance.TrySpendStamina(StaminaManager.COST_PLANT);
///   if (!ok) return; // blocked - show notification automatically
/// </summary>
public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("Maximum stamina per day.")]
    public int maxStamina = 50;

    public const int COST_TILL = 1;
    public const int COST_PLANT = 2;
    public const int COST_WATER = 1;
    public const int COST_HARVEST = 2;

    /// <summary>Current stamina value.</summary>
    public int CurrentStamina { get; private set; }

    /// <summary>Fired whenever stamina changes. Args: (current, max).</summary>
    public event Action<int, int> OnStaminaChanged;

    /// <summary>Fired when an action is blocked because stamina is zero.</summary>
    public event Action OnStaminaEmpty;

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
        CurrentStamina = maxStamina;
        TryApplySavedStamina();
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnNewDay += HandleNewDay;
        }

        LoadDataManager.OnUserLoaded += HandleUserLoaded;
        LoadDataManager.OnServerDataChanged += HandleServerDataChanged;
    }

    /// <summary>
    /// Returns true if stamina is sufficient for the given cost.
    /// </summary>
    public bool CanPerformAction(int cost) => CurrentStamina >= cost;

    /// <summary>
    /// Attempts to spend stamina. Shows a notification and returns false if insufficient.
    /// </summary>
    public bool TrySpendStamina(int cost)
    {
        if (CurrentStamina < cost)
        {
            NotificationManager.Instance?.ShowNotification(
                "Het nang luong! Nghi ngoi den ngay mai de phuc hoi.", 2.5f);
            OnStaminaEmpty?.Invoke();
            return false;
        }

        CurrentStamina -= cost;
        SyncToUserProfile();
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        AudioManager.Instance?.PlaySFX("ui_click");
        return true;
    }

    /// <summary>
    /// Restores stamina by the given amount (clamped to maxStamina).
    /// </summary>
    public void RestoreStamina(int amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, maxStamina);
        SyncToUserProfile();
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    private void HandleNewDay(int day)
    {
        CurrentStamina = maxStamina;
        SyncToUserProfile();
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        NotificationManager.Instance?.ShowNotification(
            $"Ngay {day} bat dau! Nang luong da phuc hoi hoan toan ({maxStamina}/{maxStamina}).", 3f);
        Debug.Log($"StaminaManager: Stamina restored on Day {day}");
    }

    private void HandleUserLoaded(bool success)
    {
        if (!success)
        {
            return;
        }

        TryApplySavedStamina();
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    private void HandleServerDataChanged(User serverUser)
    {
        if (serverUser == null)
        {
            return;
        }

        CurrentStamina = Mathf.Clamp(serverUser.Stamina, 0, maxStamina);
        SyncToUserProfile();
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    private void TryApplySavedStamina()
    {
        if (LoadDataManager.userInGame == null)
        {
            return;
        }

        CurrentStamina = Mathf.Clamp(LoadDataManager.userInGame.Stamina, 0, maxStamina);
        SyncToUserProfile();
    }

    private void SyncToUserProfile()
    {
        if (LoadDataManager.userInGame == null)
        {
            return;
        }

        LoadDataManager.userInGame.Stamina = Mathf.Clamp(CurrentStamina, 0, maxStamina);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnNewDay -= HandleNewDay;
        }

        LoadDataManager.OnUserLoaded -= HandleUserLoaded;
        LoadDataManager.OnServerDataChanged -= HandleServerDataChanged;
    }
}
