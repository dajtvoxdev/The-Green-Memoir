using System;
using UnityEngine;

/// <summary>
/// Manages player Gold and Diamond currency.
/// Provides earn/spend API with validation, events, and Firebase sync.
///
/// Phase 2 Feature (#9): UserInGame profile — earn/spend Gold.
///
/// Usage:
///   PlayerEconomyManager.Instance.SpendGold(50);
///   PlayerEconomyManager.Instance.EarnGold(100);
///   PlayerEconomyManager.Instance.OnGoldChanged += (newGold) => UpdateUI(newGold);
/// </summary>
public class PlayerEconomyManager : MonoBehaviour
{
    public static PlayerEconomyManager Instance { get; private set; }

    /// <summary>
    /// Fired when Gold changes. Parameter: new Gold amount.
    /// </summary>
    public event Action<int> OnGoldChanged;

    /// <summary>
    /// Fired when Diamond changes. Parameter: new Diamond amount.
    /// </summary>
    public event Action<int> OnDiamondChanged;

    /// <summary>
    /// Fired when a transaction fails. Parameter: error message.
    /// </summary>
    public event Action<string> OnTransactionFailed;

    /// <summary>
    /// Current Gold amount (reads from User data).
    /// </summary>
    public int Gold => LoadDataManager.userInGame?.Gold ?? 0;

    /// <summary>
    /// Current Diamond amount (reads from User data).
    /// </summary>
    public int Diamond => LoadDataManager.userInGame?.Diamond ?? 0;

    [Header("Settings")]
    [Tooltip("Whether to auto-save to Firebase after each transaction.")]
    public bool autoSaveToFirebase = true;

    [Tooltip("Delay before saving to Firebase (batches rapid changes).")]
    public float saveBatchDelay = 2f;

    private float pendingSaveTimer;
    private bool hasPendingSave;

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
        // Subscribe to server data changes for real-time sync (#10)
        LoadDataManager.OnServerDataChanged += OnServerDataChanged;

        // Dev mode: if userInGame is null (started PlayScene directly), create a test user
        #if UNITY_EDITOR
        if (LoadDataManager.userInGame == null)
        {
            Debug.Log("PlayerEconomy: [Dev] No user data — creating test user with 200G, 10D.");
            LoadDataManager.userInGame = new User
            {
                Name = "DevFarmer",
                Gold = 200,
                Diamond = 10
            };
            OnGoldChanged?.Invoke(Gold);
            OnDiamondChanged?.Invoke(Diamond);
        }
        #endif
    }

    /// <summary>
    /// Handles server data changes (real-time sync from Firebase listener).
    /// Updates UI to reflect server values.
    /// </summary>
    private void OnServerDataChanged(User serverUser)
    {
        if (serverUser == null) return;

        Debug.Log($"PlayerEconomy: Server sync — Gold={serverUser.Gold}, Diamond={serverUser.Diamond}");
        OnGoldChanged?.Invoke(serverUser.Gold);
        OnDiamondChanged?.Invoke(serverUser.Diamond);
    }

    void Update()
    {
        if (hasPendingSave)
        {
            pendingSaveTimer -= Time.deltaTime;
            if (pendingSaveTimer <= 0f)
            {
                hasPendingSave = false;
                SaveToFirebase();
            }
        }
    }

    // ==================== GOLD API ====================

    /// <summary>
    /// Adds Gold to the player. Returns true if successful.
    /// </summary>
    public bool EarnGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("PlayerEconomy: EarnGold amount must be positive");
            return false;
        }

        if (LoadDataManager.userInGame == null)
        {
            OnTransactionFailed?.Invoke("User data not loaded");
            return false;
        }

        LoadDataManager.userInGame.Gold += amount;
        Debug.Log($"PlayerEconomy: +{amount} Gold (total: {Gold})");
        OnGoldChanged?.Invoke(Gold);
        ScheduleSave();
        return true;
    }

    /// <summary>
    /// Spends Gold. Returns true if player has enough.
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("PlayerEconomy: SpendGold amount must be positive");
            return false;
        }

        if (LoadDataManager.userInGame == null)
        {
            OnTransactionFailed?.Invoke("User data not loaded");
            return false;
        }

        if (Gold < amount)
        {
            string msg = $"Không đủ vàng! Cần {amount}G, hiện có {Gold}G";
            Debug.Log($"PlayerEconomy: {msg}");
            OnTransactionFailed?.Invoke(msg);
            return false;
        }

        LoadDataManager.userInGame.Gold -= amount;
        Debug.Log($"PlayerEconomy: -{amount} Gold (total: {Gold})");
        OnGoldChanged?.Invoke(Gold);
        ScheduleSave();
        return true;
    }

    /// <summary>
    /// Checks if player can afford the given Gold amount.
    /// </summary>
    public bool CanAffordGold(int amount)
    {
        return Gold >= amount;
    }

    // ==================== DIAMOND API ====================

    /// <summary>
    /// Adds Diamond to the player.
    /// </summary>
    public bool EarnDiamond(int amount)
    {
        if (amount <= 0) return false;

        if (LoadDataManager.userInGame == null)
        {
            OnTransactionFailed?.Invoke("User data not loaded");
            return false;
        }

        LoadDataManager.userInGame.Diamond += amount;
        Debug.Log($"PlayerEconomy: +{amount} Diamond (total: {Diamond})");
        OnDiamondChanged?.Invoke(Diamond);
        ScheduleSave();
        return true;
    }

    /// <summary>
    /// Spends Diamond. Returns true if player has enough.
    /// </summary>
    public bool SpendDiamond(int amount)
    {
        if (amount <= 0) return false;

        if (LoadDataManager.userInGame == null)
        {
            OnTransactionFailed?.Invoke("User data not loaded");
            return false;
        }

        if (Diamond < amount)
        {
            string msg = $"Không đủ kim cương! Cần {amount}, hiện có {Diamond}";
            OnTransactionFailed?.Invoke(msg);
            return false;
        }

        LoadDataManager.userInGame.Diamond -= amount;
        Debug.Log($"PlayerEconomy: -{amount} Diamond (total: {Diamond})");
        OnDiamondChanged?.Invoke(Diamond);
        ScheduleSave();
        return true;
    }

    // ==================== FIREBASE SYNC ====================

    /// <summary>
    /// Schedules a batched save to Firebase.
    /// </summary>
    private void ScheduleSave()
    {
        if (!autoSaveToFirebase) return;

        hasPendingSave = true;
        pendingSaveTimer = saveBatchDelay;
    }

    /// <summary>
    /// Immediately saves Gold/Diamond to Firebase.
    /// Uses granular path updates to avoid overwriting entire user document.
    /// </summary>
    public void SaveToFirebase()
    {
        if (LoadDataManager.userInGame == null || LoadDataManager.firebaseUser == null) return;

        // Save the entire user object to ensure correct typing (int, not string)
        LoadDataManager.Instance?.SaveUserInGame((success, error) =>
        {
            if (success)
            {
                Debug.Log($"PlayerEconomy: Saved to Firebase (Gold={Gold}, Diamond={Diamond})");
            }
            else
            {
                Debug.LogError($"PlayerEconomy: Failed to save: {error}");
            }
        });
    }

    /// <summary>
    /// Forces immediate save (call before scene transition or logout).
    /// </summary>
    public void FlushSave()
    {
        if (hasPendingSave)
        {
            hasPendingSave = false;
            SaveToFirebase();
        }
    }

    void OnDestroy()
    {
        FlushSave();
        LoadDataManager.OnServerDataChanged -= OnServerDataChanged;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            FlushSave();
        }
    }

    private void OnApplicationQuit()
    {
        FlushSave();
    }
}
