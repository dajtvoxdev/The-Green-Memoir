using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Manages loading user data from Firebase.
/// Phase 1 Fix (T3): Added async callback support to fix race condition.
/// Phase 1 Fix (T10): Added null checks for MapInGame.
/// Phase 2 Enhancement (#10): Real-time listener for conflict-safe data sync.
/// Phase 2 Enhancement (#22): Versioned save with optimistic concurrency.
/// </summary>
public class LoadDataManager : MonoBehaviour
{
    /// <summary>
    /// Current Firebase user.
    /// </summary>
    public static FirebaseUser firebaseUser;

    /// <summary>
    /// User data loaded from Firebase.
    /// </summary>
    public static User userInGame;

    /// <summary>
    /// Event fired when user data is loaded.
    /// </summary>
    public static event Action<bool> OnUserLoaded;

    /// <summary>
    /// Event fired when server data changes while playing (real-time sync).
    /// Parameter: the updated User from server.
    /// </summary>
    public static event Action<User> OnServerDataChanged;

    /// <summary>
    /// Whether user data has been loaded.
    /// </summary>
    public static bool IsDataLoaded { get; private set; } = false;

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public static string LastErrorMessage { get; private set; }

    /// <summary>
    /// Whether the real-time listener is active.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    private DatabaseReference reference;
    private DatabaseReference userRef;
    private bool isInitialLoad = true;

    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static LoadDataManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            FirebaseApp app = FirebaseApp.DefaultInstance;
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            
            firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
            
            if (firebaseUser != null)
            {
                GetUserInGame();
            }
            else
            {
                Debug.LogError("LoadDataManager: No Firebase user logged in!");
                OnUserLoaded?.Invoke(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // No update needed
    }
    
    /// <summary>
    /// Loads user data from Firebase with callback.
    /// Phase 1: Added async callback support (T3 fix).
    /// </summary>
    /// <param name="onComplete">Callback with success status</param>
    public void GetUserInGame(Action<bool> onComplete = null)
    {
        if (firebaseUser == null)
        {
            Debug.LogError("LoadDataManager: Firebase user is null!");
            onComplete?.Invoke(false);
            OnUserLoaded?.Invoke(false);
            return;
        }
        
        reference.Child("Users").Child(firebaseUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                
                if (snapshot.Value != null)
                {
                    try
                    {
                        userInGame = JsonConvert.DeserializeObject<User>(snapshot.Value.ToString());
                        IsDataLoaded = true;
                        LastErrorMessage = null;
                        Debug.Log("LoadDataManager: User data loaded successfully");
                        Debug.Log("User in game: " + userInGame?.ToString());
                        
                        // Null check for MapInGame (T10 fix)
                        if (userInGame?.MapInGame?.lstTilemapDetail == null)
                        {
                            Debug.LogWarning("LoadDataManager: MapInGame is null or empty. New user?");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"LoadDataManager: Failed to deserialize user data: {ex.Message}");
                        IsDataLoaded = false;
                        LastErrorMessage = ex.Message;
                    }
                }
                else
                {
                    // No data exists for this user - create default
                    Debug.Log("LoadDataManager: No existing user data. Creating new user.");
                    userInGame = new User();
                    IsDataLoaded = true;
                }
                
                onComplete?.Invoke(IsDataLoaded);
                OnUserLoaded?.Invoke(IsDataLoaded);

                // Start real-time listener after successful initial load
                if (IsDataLoaded && !IsListening)
                {
                    StartListening();
                }
            }
            else
            {
                string errorMessage = task.Exception?.GetBaseException().Message ?? "Unknown error";
                Debug.LogError("LoadDataManager: Failed to load user data: " + errorMessage);
                IsDataLoaded = false;
                LastErrorMessage = errorMessage;

                onComplete?.Invoke(false);
                OnUserLoaded?.Invoke(false);
            }
        });
    }
    
    /// <summary>
    /// Saves the current user data to Firebase.
    /// Phase 2 Enhancement (#22): Uses versioned save via FirebaseTransactionManager
    /// when available, falls back to direct write.
    /// </summary>
    /// <param name="onComplete">Callback with success status</param>
    public void SaveUserInGame(Action<bool, string> onComplete = null)
    {
        if (firebaseUser == null)
        {
            Debug.LogError("LoadDataManager: Firebase user is null!");
            onComplete?.Invoke(false, "No Firebase user");
            return;
        }

        if (userInGame == null)
        {
            Debug.LogError("LoadDataManager: User data is null!");
            onComplete?.Invoke(false, "No user data");
            return;
        }

        // Try versioned save for data integrity
        if (FirebaseTransactionManager.Instance != null)
        {
            SaveWithVersion(onComplete);
        }
        else
        {
            // Fallback to direct write
            string jsonData = userInGame.ToString();
            FirebaseDatabaseManager.Instance?.WriteDatabase("Users/" + firebaseUser.UserId, jsonData, onComplete);
        }
    }

    /// <summary>
    /// Saves user data with version check for conflict detection.
    /// Phase 2 Feature (#22): Optimistic concurrency control.
    /// </summary>
    private async void SaveWithVersion(Action<bool, string> onComplete = null)
    {
        try
        {
            var result = await FirebaseTransactionManager.Instance.SaveUserWithVersion(userInGame, firebaseUser.UserId);

            if (result.Success)
            {
                Debug.Log($"LoadDataManager: Versioned save succeeded (v{userInGame.Version})");
                onComplete?.Invoke(true, null);
            }
            else
            {
                Debug.LogWarning($"LoadDataManager: Versioned save failed: {result.ErrorMessage}");
                onComplete?.Invoke(false, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"LoadDataManager: SaveWithVersion exception: {ex.Message}");
            onComplete?.Invoke(false, ex.Message);
        }
    }
    
    /// <summary>
    /// Starts a real-time listener on the user's data path.
    /// Any changes from server will fire OnServerDataChanged event.
    /// Phase 2 Feature (#10): Conflict-safe real-time sync.
    /// </summary>
    public void StartListening()
    {
        if (firebaseUser == null || IsListening) return;

        userRef = reference.Child("Users").Child(firebaseUser.UserId);
        isInitialLoad = true;

        userRef.ValueChanged += OnValueChanged;
        IsListening = true;

        Debug.Log("LoadDataManager: Started real-time listener");
    }

    /// <summary>
    /// Stops the real-time listener.
    /// </summary>
    public void StopListening()
    {
        if (userRef != null && IsListening)
        {
            userRef.ValueChanged -= OnValueChanged;
            IsListening = false;
            Debug.Log("LoadDataManager: Stopped real-time listener");
        }
    }

    /// <summary>
    /// Handles real-time value changes from Firebase.
    /// Skips the initial load event (already handled by GetUserInGame).
    /// </summary>
    private void OnValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"LoadDataManager: Listener error: {args.DatabaseError.Message}");
            return;
        }

        // Skip initial event (data already loaded via GetUserInGame)
        if (isInitialLoad)
        {
            isInitialLoad = false;
            return;
        }

        if (args.Snapshot?.Value == null) return;

        try
        {
            User serverUser = JsonConvert.DeserializeObject<User>(args.Snapshot.Value.ToString());

            if (serverUser == null) return;

            // Check if server version is newer than local
            if (userInGame != null && serverUser.Version > userInGame.Version)
            {
                Debug.Log($"LoadDataManager: Server data updated (v{serverUser.Version} > local v{userInGame.Version})");

                // Update local data with server data (server-wins strategy)
                userInGame = serverUser;
                OnServerDataChanged?.Invoke(serverUser);

                NotificationManager.Instance?.ShowNotification("Data synced from server.", 2f);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"LoadDataManager: Failed to parse server update: {ex.Message}");
        }
    }

    /// <summary>
    /// Reloads user data from Firebase (manual refresh).
    /// Useful when user suspects stale data.
    /// </summary>
    public void ReloadUserData(Action<bool> onComplete = null)
    {
        Debug.Log("LoadDataManager: Manual reload requested");
        GetUserInGame(onComplete);
    }

    /// <summary>
    /// Resets the loaded data (for logout or scene transition).
    /// </summary>
    public static void Reset()
    {
        // Stop listener before reset
        if (Instance != null)
        {
            Instance.StopListening();
        }

        userInGame = null;
        IsDataLoaded = false;
        LastErrorMessage = null;
    }

    private void OnDestroy()
    {
        StopListening();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
