using System;
using UnityEngine;

/// <summary>
/// Manages player stamina — a finite daily resource consumed by farm actions.
/// Fully restores at the start of each new in-game day.
///
/// Phase 3 Feature (#33): Energy / Stamina System.
///
/// Action costs:
///   Till = 1, Plant = 2, Water = 1, Harvest = 2
///
/// Usage:
///   bool ok = StaminaManager.Instance.TrySpendStamina(StaminaManager.COST_PLANT);
///   if (!ok) return; // blocked — show notification automatically
/// </summary>
public class StaminaManager : MonoBehaviour
{
    public static StaminaManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("Maximum stamina per day.")]
    public int maxStamina = 50;

    // ── Action costs (constants, read by PlayerFarmController)
    public const int COST_TILL    = 1;
    public const int COST_PLANT   = 2;
    public const int COST_WATER   = 1;
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
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnNewDay += HandleNewDay;
        }
    }

    // ==================== PUBLIC API ====================

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
                "Hết năng lượng! Nghỉ ngơi đến ngày mai để phục hồi.", 2.5f);
            OnStaminaEmpty?.Invoke();
            return false;
        }

        CurrentStamina -= cost;
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
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
    }

    // ==================== PRIVATE ====================

    private void HandleNewDay(int day)
    {
        CurrentStamina = maxStamina;
        OnStaminaChanged?.Invoke(CurrentStamina, maxStamina);
        NotificationManager.Instance?.ShowNotification(
            $"Ngày {day} bắt đầu! Năng lượng đã phục hồi hoàn toàn ({maxStamina}/{maxStamina}).", 3f);
        Debug.Log($"StaminaManager: Stamina restored on Day {day}");
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
    }
}
