using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
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
    public static bool HasExistingCloudProfile { get; private set; } = false;

    /// <summary>
    /// Error message if loading failed.
    /// </summary>
    public static string LastErrorMessage { get; private set; }

    /// <summary>
    /// Whether the real-time listener is active.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    /// <summary>
    /// Whether a Firebase profile load is currently in progress.
    /// </summary>
    public bool IsLoadInProgress => isLoadInFlight;

    private DatabaseReference reference;
    private DatabaseReference userRef;
    private bool isInitialLoad = true;
    private FirebaseAuth auth;
    private bool isLoadInFlight = false;

    private const int DefaultStarterGold = 200;
    private const int DefaultStarterDiamond = 10;

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
            auth = FirebaseAuth.DefaultInstance;
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            auth.StateChanged += OnAuthStateChanged;

            RefreshFirebaseUser();

            if (firebaseUser != null)
            {
                GetUserInGame();
            }
            else
            {
                Debug.LogWarning("LoadDataManager: No Firebase user yet (will retry on scene load).");
            }
        }
        else
        {
            // Existing instance found — refresh user in case login just happened
            RefreshFirebaseUser();
            if (firebaseUser != null && !IsDataLoaded)
            {
                Instance.GetUserInGame();
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Re-fetches the current Firebase user. Call after login completes.
    /// </summary>
    public void RefreshFirebaseUser()
    {
        firebaseUser = FirebaseAuth.DefaultInstance.CurrentUser;
        if (firebaseUser != null)
        {
            Debug.Log($"LoadDataManager: Firebase user = {firebaseUser.UserId}");
        }
        else
        {
            Debug.Log("LoadDataManager: Firebase user is still null.");
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
        if (isLoadInFlight)
        {
            Debug.Log("LoadDataManager: User data load already in progress.");
            return;
        }

        if (firebaseUser == null)
        {
            Debug.LogError("LoadDataManager: Firebase user is null!");
            onComplete?.Invoke(false);
            OnUserLoaded?.Invoke(false);
            return;
        }

        isLoadInFlight = true;

        reference.Child("Users").Child(firebaseUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            isLoadInFlight = false;

            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                try
                {
                    bool shouldSaveProfile;
                    bool isNewUser;
                    userInGame = BuildUserFromSnapshot(task.Result, out shouldSaveProfile, out isNewUser);

                    IsDataLoaded = userInGame != null;
                    HasExistingCloudProfile = !isNewUser;
                    LastErrorMessage = null;

                    if (IsDataLoaded)
                    {
                        if (isNewUser)
                        {
                            Debug.Log("LoadDataManager: No existing gameplay profile. Creating default starter profile.");
                        }
                        else
                        {
                            Debug.Log("LoadDataManager: User data loaded successfully");
                        }

                        Debug.Log("User in game: " + userInGame);

                        #if UNITY_EDITOR
                        if (userInGame != null && userInGame.Gold <= 0)
                        {
                            Debug.Log("LoadDataManager: [Dev] Granting starter gold (200G, 10D) to existing user.");
                            userInGame.Gold = DefaultStarterGold;
                            userInGame.Diamond = Mathf.Max(userInGame.Diamond, DefaultStarterDiamond);
                            shouldSaveProfile = true;
                        }
                        #endif

                        if (shouldSaveProfile)
                        {
                            if (!isNewUser && !HasUsableMap(userInGame))
                            {
                                Debug.LogWarning("LoadDataManager: Skipping profile migration save because the loaded cloud profile is missing MapInGame.");
                            }
                            else
                            {
                                SaveUserInGame((success, error) =>
                                {
                                    if (success)
                                    {
                                        Debug.Log("LoadDataManager: User profile migrated to the new schema.");
                                    }
                                    else
                                    {
                                        Debug.LogWarning($"LoadDataManager: Failed to migrate user profile: {error}");
                                    }
                                });
                            }
                        }

                        if (userInGame?.MapInGame?.lstTilemapDetail == null)
                        {
                            if (isNewUser)
                            {
                                Debug.LogWarning("LoadDataManager: MapInGame is null or empty for a new user. TileMapManager will create a fresh map.");
                            }
                            else
                            {
                                Debug.LogWarning("LoadDataManager: Existing cloud profile is missing MapInGame.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"LoadDataManager: Failed to resolve user data: {ex.Message}");
                    IsDataLoaded = false;
                    HasExistingCloudProfile = false;
                    LastErrorMessage = ex.Message;
                }

                onComplete?.Invoke(IsDataLoaded);
                OnUserLoaded?.Invoke(IsDataLoaded);

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
                HasExistingCloudProfile = false;
                LastErrorMessage = errorMessage;

                onComplete?.Invoke(false);
                OnUserLoaded?.Invoke(false);
            }
        });
    }

    private void OnAuthStateChanged(object sender, EventArgs args)
    {
        FirebaseUser previousUser = firebaseUser;
        RefreshFirebaseUser();

        if (firebaseUser == null)
        {
            if (previousUser != null)
            {
                Debug.Log("LoadDataManager: Auth state changed to signed-out.");
                StopListening();
                userInGame = null;
                IsDataLoaded = false;
                HasExistingCloudProfile = false;
            }

            return;
        }

        bool userChanged = previousUser == null || previousUser.UserId != firebaseUser.UserId;
        if (userChanged)
        {
            Debug.Log($"LoadDataManager: Auth state changed. Preparing data load for {firebaseUser.UserId}");
            StopListening();
            userInGame = null;
            IsDataLoaded = false;
            HasExistingCloudProfile = false;
            LastErrorMessage = null;
        }

        if (!IsDataLoaded && !isLoadInFlight)
        {
            GetUserInGame();
        }
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

        if (HasExistingCloudProfile && !HasUsableMap(userInGame))
        {
            const string error = "Local MapInGame is missing. Refusing to overwrite an existing cloud profile.";
            Debug.LogWarning($"LoadDataManager: {error}");
            onComplete?.Invoke(false, error);
            return;
        }

        if (FirebaseTransactionManager.Instance != null)
        {
            SaveWithVersion(onComplete);
        }
        else
        {
            string jsonData = userInGame.ToString();
            FirebaseDatabaseManager.Instance?.WriteDatabase(
                FirebaseUserPaths.GetUserProfilePath(firebaseUser.UserId),
                jsonData,
                onComplete);
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
    /// Starts a real-time listener on the user's root path.
    /// Listening to the root keeps compatibility with sibling nodes such as hasPurchased.
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

        if (isInitialLoad)
        {
            isInitialLoad = false;
            return;
        }

        if (args.Snapshot?.Value == null) return;

        try
        {
            bool shouldSaveProfile;
            bool isNewUser;
            User serverUser = BuildUserFromSnapshot(args.Snapshot, out shouldSaveProfile, out isNewUser);

            if (serverUser == null) return;

            bool serverMissingMap = serverUser.MapInGame?.lstTilemapDetail == null;
            bool localHasMap = userInGame?.MapInGame?.lstTilemapDetail != null;
            if (serverMissingMap && localHasMap)
            {
                Debug.LogWarning("LoadDataManager: Listener update is missing MapInGame. Preserving local map and ignoring profile normalization for this event.");
                serverUser.MapInGame = userInGame.MapInGame;
                shouldSaveProfile = false;
            }

            bool localMissing = userInGame == null;
            bool hasNewerVersion = userInGame != null && serverUser.Version > userInGame.Version;
            bool purchaseChanged = userInGame != null && serverUser.HasPurchased != userInGame.HasPurchased;

            if (localMissing || hasNewerVersion || purchaseChanged)
            {
                if (hasNewerVersion)
                {
                    Debug.Log($"LoadDataManager: Server data updated (v{serverUser.Version} > local v{userInGame.Version})");
                }

                userInGame = serverUser;
                OnServerDataChanged?.Invoke(serverUser);

                if (hasNewerVersion)
                {
                    NotificationManager.Instance?.ShowNotification("Dữ liệu đã đồng bộ từ máy chủ.", 2f);
                }
            }

            if (shouldSaveProfile)
            {
                if (!isNewUser && !HasUsableMap(serverUser))
                {
                    Debug.LogWarning("LoadDataManager: Skipping normalized listener save because the server payload is missing MapInGame.");
                }
                else
                {
                    SaveUserInGame((success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogWarning($"LoadDataManager: Failed to persist normalized listener data: {error}");
                        }
                    });
                }
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
        if (Instance != null)
        {
            Instance.StopListening();
        }

        userInGame = null;
        IsDataLoaded = false;
        HasExistingCloudProfile = false;
        LastErrorMessage = null;
    }

    private void OnDestroy()
    {
        StopListening();

        if (auth != null)
        {
            auth.StateChanged -= OnAuthStateChanged;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private User BuildUserFromSnapshot(DataSnapshot snapshot, out bool shouldSaveProfile, out bool isNewUser)
    {
        shouldSaveProfile = false;
        isNewUser = false;

        if (snapshot?.Value == null)
        {
            shouldSaveProfile = true;
            isNewUser = true;
            return CreateDefaultUserProfile();
        }

        string rawRootJson = snapshot.GetRawJsonValue();
        string normalizedRootJson = FirebaseJsonUtility.NormalizeReadValue(rawRootJson);
        Debug.Log($"LoadDataManager: Snapshot raw json = {rawRootJson}");
        Debug.Log($"LoadDataManager: Snapshot normalized json = {normalizedRootJson}");
        if (string.IsNullOrEmpty(normalizedRootJson) || normalizedRootJson == "null" || normalizedRootJson == "{}")
        {
            shouldSaveProfile = true;
            isNewUser = true;
            return CreateDefaultUserProfile();
        }

        JToken rootToken;
        try
        {
            rootToken = JToken.Parse(normalizedRootJson);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LoadDataManager: Root user payload is invalid JSON. Recreating profile. {ex.Message}");
            shouldSaveProfile = true;
            isNewUser = true;
            return CreateDefaultUserProfile();
        }

        bool hasProfileNode;
        bool usedLegacyRootProfile;
        User resolvedUser = TryExtractUser(rootToken, out hasProfileNode, out usedLegacyRootProfile);

        if (NeedsBootstrapProfile(resolvedUser))
        {
            shouldSaveProfile = true;
            isNewUser = true;
            resolvedUser = CreateDefaultUserProfile();
        }

        bool rootHasPurchased = TryGetBoolean(rootToken["hasPurchased"]);
        if (rootHasPurchased && resolvedUser != null && !resolvedUser.HasPurchased)
        {
            resolvedUser.HasPurchased = true;
            shouldSaveProfile = true;
        }

        if (resolvedUser != null)
        {
            if (resolvedUser.MapInGame == null)
            {
                if (isNewUser)
                {
                    resolvedUser.MapInGame = new Map();
                    shouldSaveProfile = true;
                }
                else if (HasLegacyMapNode(rootToken))
                {
                    Debug.LogWarning("LoadDataManager: Found legacy map data under Users/{uid}/map. It does not match the current schema, so a fresh tilemap will be created.");
                }
                else
                {
                    Debug.LogWarning("LoadDataManager: Existing cloud profile is missing MapInGame. Automatic profile migration is disabled to avoid overwriting map data.");
                }
            }

            if (string.IsNullOrWhiteSpace(resolvedUser.Name))
            {
                resolvedUser.Name = firebaseUser?.DisplayName ?? "Farmer";
                shouldSaveProfile = true;
            }
        }

        if (!hasProfileNode || usedLegacyRootProfile)
        {
            shouldSaveProfile = true;
        }

        return resolvedUser;
    }

    private User TryExtractUser(JToken rootToken, out bool hasProfileNode, out bool usedLegacyRootProfile)
    {
        hasProfileNode = false;
        usedLegacyRootProfile = false;

        if (rootToken == null)
        {
            return null;
        }

        if (rootToken.Type == JTokenType.Object)
        {
            JObject rootObject = (JObject)rootToken;
            JToken profileToken = rootObject["profile"];
            if (profileToken != null)
            {
                hasProfileNode = true;
                return DeserializeUserToken(profileToken);
            }
        }

        if (LooksLikeUserToken(rootToken))
        {
            usedLegacyRootProfile = true;
            return DeserializeUserToken(rootToken);
        }

        return null;
    }

    private User DeserializeUserToken(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        token = NormalizeUserToken(token);

        string json = token.Type == JTokenType.String
            ? FirebaseJsonUtility.NormalizeReadValue(token.ToString(Formatting.None))
            : token.ToString(Formatting.None);

        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        try
        {
            User user = BuildUserFromToken(token);
            if (user == null)
            {
                user = JsonConvert.DeserializeObject<User>(json);
                HydrateUserFromToken(user, token);
            }
            return user;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"LoadDataManager: Failed to deserialize user token: {ex.Message}");
            return null;
        }
    }

    private User BuildUserFromToken(JToken token)
    {
        if (token == null || token.Type != JTokenType.Object)
        {
            return null;
        }

        JObject obj = (JObject)token;
        if (GetPropertyValueIgnoreCase(obj, "profile") is JObject nestedProfile
            && GetPropertyValueIgnoreCase(obj, "Name") == null
            && GetPropertyValueIgnoreCase(obj, "Gold") == null)
        {
            obj = nestedProfile;
        }

        User user = new User
        {
            Name = ReadString(GetPropertyValueIgnoreCase(obj, "Name")),
            Gold = ReadInt(GetPropertyValueIgnoreCase(obj, "Gold")),
            Diamond = ReadInt(GetPropertyValueIgnoreCase(obj, "Diamond")),
            Stamina = ReadInt(GetPropertyValueIgnoreCase(obj, "Stamina"), 50),
            Version = ReadLong(GetPropertyValueIgnoreCase(obj, "Version")),
            HasPurchased = TryGetBoolean(GetPropertyValueIgnoreCase(obj, "hasPurchased"))
        };

        JToken mapToken = GetPropertyValueIgnoreCase(obj, "MapInGame");
        if (mapToken != null && mapToken.Type != JTokenType.Null)
        {
            Debug.Log($"LoadDataManager: BuildUserFromToken MapInGame token type = {mapToken.Type}");
            user.MapInGame = new Map
            {
                lstTilemapDetail = TryDeserializeTileList(GetPropertyValueIgnoreCase(mapToken as JObject, "lstTilemapDetail"))
            };

            Debug.Log($"LoadDataManager: BuildUserFromToken tile count = {user.MapInGame?.lstTilemapDetail?.Count ?? -1}");
        }
        else
        {
            Debug.LogWarning($"LoadDataManager: BuildUserFromToken did not find a valid MapInGame token. Keys = {string.Join(",", obj.Properties())}");
        }

        return user;
    }

    private void HydrateUserFromToken(User user, JToken token)
    {
        if (user == null || token == null)
        {
            return;
        }

        if (token.Type == JTokenType.String)
        {
            string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString(Formatting.None));
            if (!string.IsNullOrEmpty(normalized))
            {
                try
                {
                    token = JToken.Parse(normalized);
                }
                catch
                {
                    return;
                }
            }
        }

        if (token.Type != JTokenType.Object)
        {
            return;
        }

        JObject obj = (JObject)token;
        JToken tileListToken = obj["MapInGame"]?["lstTilemapDetail"];
        List<TilemapDetail> tiles = TryDeserializeTileList(tileListToken);

        if (tiles != null)
        {
            user.MapInGame ??= new Map();
            user.MapInGame.lstTilemapDetail = tiles;
        }
    }

    private JToken NormalizeUserToken(JToken token)
    {
        if (token == null)
        {
            return token;
        }

        if (token.Type == JTokenType.String)
        {
            string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString(Formatting.None));
            if (string.IsNullOrEmpty(normalized))
            {
                return token;
            }

            try
            {
                token = JToken.Parse(normalized);
            }
            catch
            {
                return token;
            }
        }

        if (token.Type != JTokenType.Object)
        {
            return token;
        }

        JObject clone = (JObject)token.DeepClone();
        JToken tileListToken = clone["MapInGame"]?["lstTilemapDetail"];
        if (tileListToken is JObject tileListObject)
        {
            clone["MapInGame"]["lstTilemapDetail"] = ConvertIndexedObjectToArray(tileListObject);
        }

        return clone;
    }

    private List<TilemapDetail> TryDeserializeTileList(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            Debug.LogWarning("LoadDataManager: TryDeserializeTileList received a null token.");
            return null;
        }

        if (token.Type == JTokenType.String)
        {
            string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString(Formatting.None));
            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            try
            {
                token = JToken.Parse(normalized);
                Debug.Log($"LoadDataManager: TryDeserializeTileList parsed string token into {token.Type}.");
            }
            catch
            {
                Debug.LogWarning("LoadDataManager: TryDeserializeTileList could not parse string token.");
                return null;
            }
        }

        if (token is JArray array)
        {
            Debug.Log($"LoadDataManager: TryDeserializeTileList found JArray with {array.Count} entries.");
            try
            {
                List<TilemapDetail> tiles = array.ToObject<List<TilemapDetail>>();
                Debug.Log($"LoadDataManager: TryDeserializeTileList ToObject success, count = {tiles?.Count ?? -1}");
                return tiles;
            }
            catch
            {
                Debug.LogWarning("LoadDataManager: TryDeserializeTileList ToObject failed for JArray. Falling back to manual parse.");
                return DeserializeTileListManually(array);
            }
        }

        if (token is JObject indexedObject)
        {
            Debug.Log($"LoadDataManager: TryDeserializeTileList found JObject with {indexedObject.Count} indexed entries.");
            JArray normalizedArray = ConvertIndexedObjectToArray(indexedObject);
            try
            {
                List<TilemapDetail> tiles = normalizedArray.ToObject<List<TilemapDetail>>();
                Debug.Log($"LoadDataManager: TryDeserializeTileList indexed ToObject success, count = {tiles?.Count ?? -1}");
                return tiles;
            }
            catch
            {
                Debug.LogWarning("LoadDataManager: TryDeserializeTileList indexed ToObject failed. Falling back to manual parse.");
                return DeserializeTileListManually(normalizedArray);
            }
        }

        Debug.LogWarning($"LoadDataManager: TryDeserializeTileList received unsupported token type {token.Type}.");
        return null;
    }

    private List<TilemapDetail> DeserializeTileListManually(JArray array)
    {
        if (array == null)
        {
            return null;
        }

        var tiles = new List<TilemapDetail>(array.Count);
        foreach (JToken item in array)
        {
            if (item is not JObject tileObj)
            {
                continue;
            }

            tiles.Add(new TilemapDetail
            {
                x = ReadInt(tileObj["x"]),
                y = ReadInt(tileObj["y"]),
                tilemapState = (TilemapState)ReadInt(tileObj["tilemapState"]),
                cropId = ReadString(tileObj["cropId"]),
                plantedAt = ReadLong(tileObj["plantedAt"]),
                growthStage = ReadInt(tileObj["growthStage"]),
                lastWateredAt = ReadLong(tileObj["lastWateredAt"])
            });
        }

        Debug.Log($"LoadDataManager: DeserializeTileListManually parsed {tiles.Count} tiles.");
        return tiles.Count > 0 ? tiles : null;
    }

    private JArray ConvertIndexedObjectToArray(JObject indexedObject)
    {
        var orderedItems = new List<(int index, JToken value)>();

        foreach (JProperty property in indexedObject.Properties())
        {
            if (!int.TryParse(property.Name, out int index))
            {
                continue;
            }

            orderedItems.Add((index, property.Value));
        }

        orderedItems.Sort((left, right) => left.index.CompareTo(right.index));

        JArray array = new JArray();
        foreach (var item in orderedItems)
        {
            array.Add(item.value);
        }

        return array;
    }

    private bool LooksLikeUserToken(JToken token)
    {
        if (token == null)
        {
            return false;
        }

        if (token.Type == JTokenType.String)
        {
            string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString(Formatting.None));
            if (string.IsNullOrEmpty(normalized))
            {
                return false;
            }

            try
            {
                token = JToken.Parse(normalized);
            }
            catch
            {
                return false;
            }
        }

        if (token.Type != JTokenType.Object)
        {
            return false;
        }

        JObject obj = (JObject)token;
        return obj["Name"] != null
            || obj["Gold"] != null
            || obj["Diamond"] != null
            || obj["MapInGame"] != null
            || obj["Version"] != null
            || obj["hasPurchased"] != null;
    }

    private bool NeedsBootstrapProfile(User candidate)
    {
        if (candidate == null)
        {
            return true;
        }

        return candidate.MapInGame == null
            && string.IsNullOrWhiteSpace(candidate.Name)
            && candidate.Gold == 0
            && candidate.Diamond == 0
            && candidate.Version == 0;
    }

    private bool HasUsableMap(User candidate)
    {
        return candidate?.MapInGame?.lstTilemapDetail != null
            && candidate.MapInGame.lstTilemapDetail.Count > 0;
    }

    private bool HasLegacyMapNode(JToken rootToken)
    {
        return rootToken is JObject rootObject && rootObject["map"] != null;
    }

    private bool TryGetBoolean(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return false;
        }

        try
        {
            if (token.Type == JTokenType.Boolean)
            {
                return token.Value<bool>();
            }

            if (token.Type == JTokenType.String)
            {
                string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString(Formatting.None));
                return bool.TryParse(normalized, out bool result) && result;
            }

            return token.Value<bool>();
        }
        catch
        {
            return false;
        }
    }

    private string ReadString(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return null;
        }

        try
        {
            return token.Type == JTokenType.String
                ? FirebaseJsonUtility.NormalizeReadValue(token.ToString(Formatting.None))
                : token.Value<string>();
        }
        catch
        {
            return null;
        }
    }

    private JToken GetPropertyValueIgnoreCase(JObject obj, string propertyName)
    {
        if (obj == null || string.IsNullOrEmpty(propertyName))
        {
            return null;
        }

        foreach (JProperty property in obj.Properties())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        return null;
    }

    private int ReadInt(JToken token, int fallback = 0)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return fallback;
        }

        try
        {
            if (token.Type == JTokenType.Integer)
            {
                return token.Value<int>();
            }

            string normalized = ReadString(token);
            return int.TryParse(normalized, out int value) ? value : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private long ReadLong(JToken token, long fallback = 0)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return fallback;
        }

        try
        {
            if (token.Type == JTokenType.Integer)
            {
                return token.Value<long>();
            }

            string normalized = ReadString(token);
            return long.TryParse(normalized, out long value) ? value : fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private User CreateDefaultUserProfile()
    {
        return new User
        {
            Name = firebaseUser?.DisplayName ?? "Farmer",
            Gold = DefaultStarterGold,
            Diamond = DefaultStarterDiamond,
            MapInGame = new Map()
        };
    }
}
