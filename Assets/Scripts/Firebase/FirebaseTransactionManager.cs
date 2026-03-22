using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Manages Firebase transactions with retry logic, exponential backoff,
/// and atomic operations for data integrity.
///
/// Phase 2 Enhancement (#22): Added RunAtomicTransaction using Firebase native API,
/// versioned saves for optimistic concurrency, and granular path updates.
///
/// Usage:
///   // Simple retry write
///   await FirebaseTransactionManager.Instance.WriteWithRetry(path, data);
///
///   // Atomic transaction (compare-and-swap)
///   await FirebaseTransactionManager.Instance.RunAtomicTransaction(path, currentData => {
///       var obj = JsonConvert.DeserializeObject&lt;MyType&gt;(currentData);
///       obj.Value += 10;
///       return JsonConvert.SerializeObject(obj);
///   });
///
///   // Versioned user save
///   await FirebaseTransactionManager.Instance.SaveUserWithVersion(user);
/// </summary>
public class FirebaseTransactionManager : MonoBehaviour
{
    public static FirebaseTransactionManager Instance { get; private set; }

    private DatabaseReference reference;

    // Exponential backoff configuration
    private const float InitialDelay = 0.5f;
    private const float MaxDelay = 10f;
    private const float BackoffMultiplier = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            reference = FirebaseDatabase.DefaultInstance.RootReference;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Writes data to Firebase with automatic retry on failure.
    /// Uses exponential backoff between retries.
    /// </summary>
    public async Task<FirebaseResponse<bool>> WriteWithRetry(string path, string data, int maxRetries = 3)
    {
        float delay = InitialDelay;
        string jsonPayload = FirebaseJsonUtility.PrepareForWrite(data);

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var task = reference.Child(path).SetRawJsonValueAsync(jsonPayload);
                await task;

                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"FirebaseTransaction: Write succeeded at {path} (attempt {attempt + 1})");
                    return FirebaseResponse<bool>.Ok(true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"FirebaseTransaction: Write failed at {path} (attempt {attempt + 1}/{maxRetries}): {ex.Message}");
            }

            if (attempt < maxRetries - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(delay));
                delay = Mathf.Min(delay * BackoffMultiplier, MaxDelay);
            }
        }

        Debug.LogError($"FirebaseTransaction: Write failed after {maxRetries} attempts at {path}");
        return FirebaseResponse<bool>.Fail($"Failed after {maxRetries} retries");
    }

    /// <summary>
    /// Reads data from Firebase with automatic retry on failure.
    /// </summary>
    public async Task<FirebaseResponse<T>> ReadWithRetry<T>(string path, int maxRetries = 3)
    {
        float delay = InitialDelay;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var task = reference.Child(path).GetValueAsync();
                await task;

                if (task.IsCompleted && !task.IsFaulted && task.Result != null)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Value != null)
                    {
                        string rawJson = FirebaseJsonUtility.NormalizeReadValue(snapshot.GetRawJsonValue());
                        T data = ConvertValue<T>(rawJson);
                        Debug.Log($"FirebaseTransaction: Read succeeded from {path} (attempt {attempt + 1})");
                        return FirebaseResponse<T>.Ok(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"FirebaseTransaction: Read failed from {path} (attempt {attempt + 1}/{maxRetries}): {ex.Message}");
            }

            if (attempt < maxRetries - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(delay));
                delay = Mathf.Min(delay * BackoffMultiplier, MaxDelay);
            }
        }

        Debug.LogError($"FirebaseTransaction: Read failed after {maxRetries} attempts from {path}");
        return FirebaseResponse<T>.Fail($"Failed after {maxRetries} retries");
    }

    /// <summary>
    /// Runs a true atomic transaction using Firebase's native RunTransaction API.
    /// The update function receives current data and returns new data.
    /// Firebase automatically retries if data changes between read and write.
    /// </summary>
    /// <param name="path">Firebase path to update atomically.</param>
    /// <param name="updateFunc">Function: current JSON string → new JSON string. Return null to abort.</param>
    /// <param name="maxRetries">Max retry attempts for network failures (Firebase handles concurrency internally).</param>
    public async Task<FirebaseResponse<bool>> RunAtomicTransaction(string path, Func<string, string> updateFunc, int maxRetries = 3)
    {
        float delay = InitialDelay;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var transactionTask = reference.Child(path).RunTransaction(mutableData =>
                {
                    string currentJson = FirebaseJsonUtility.ConvertValueToJson(mutableData.Value);
                    string newJson = updateFunc(currentJson);

                    if (newJson == null)
                    {
                        // Abort transaction
                        return TransactionResult.Abort();
                    }

                    // Parse the new JSON to set as proper object (not string)
                    try
                    {
                        object parsed = JsonConvert.DeserializeObject(newJson);
                        mutableData.Value = ConvertToFirebaseValue(parsed);
                    }
                    catch
                    {
                        mutableData.Value = newJson;
                    }

                    return TransactionResult.Success(mutableData);
                });

                await transactionTask;

                if (transactionTask.IsCompleted && !transactionTask.IsFaulted)
                {
                    Debug.Log($"FirebaseTransaction: Atomic transaction succeeded at {path}");
                    return FirebaseResponse<bool>.Ok(true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"FirebaseTransaction: Atomic transaction failed at {path} (attempt {attempt + 1}/{maxRetries}): {ex.Message}");
            }

            if (attempt < maxRetries - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(delay));
                delay = Mathf.Min(delay * BackoffMultiplier, MaxDelay);
            }
        }

        Debug.LogError($"FirebaseTransaction: Atomic transaction failed after {maxRetries} attempts at {path}");
        return FirebaseResponse<bool>.Fail($"Atomic transaction failed after {maxRetries} retries");
    }

    /// <summary>
    /// Saves user data with optimistic version check.
    /// Reads current version, increments, and writes atomically.
    /// Rejects if local version is stale (another client wrote first).
    /// </summary>
    public async Task<FirebaseResponse<bool>> SaveUserWithVersion(User localUser, string userId)
    {
        if (localUser == null || string.IsNullOrEmpty(userId))
        {
            return FirebaseResponse<bool>.Fail("Invalid user or userId");
        }

        string path = FirebaseUserPaths.GetUserProfilePath(userId);

        var result = await RunAtomicTransaction(path, currentJson =>
        {
            if (string.IsNullOrEmpty(currentJson) || currentJson == "null" || currentJson == "{}")
            {
                // No existing data — first save, set version to 1
                localUser.Version = 1;
                return JsonConvert.SerializeObject(localUser);
            }

            try
            {
                User serverUser = JsonConvert.DeserializeObject<User>(currentJson);

                // Version check: reject if server has newer version
                if (serverUser != null && serverUser.Version > localUser.Version)
                {
                    Debug.LogWarning($"FirebaseTransaction: Version conflict! Local={localUser.Version}, Server={serverUser.Version}");
                    return null; // Abort — stale data
                }

                // Increment version and save
                localUser.Version = (serverUser?.Version ?? 0) + 1;
                return JsonConvert.SerializeObject(localUser);
            }
            catch (Exception ex)
            {
                Debug.LogError($"FirebaseTransaction: Version check failed: {ex.Message}");
                return null; // Abort on parse error
            }
        });

        if (result.Success)
        {
            Debug.Log($"FirebaseTransaction: User saved with version {localUser.Version}");
        }
        else
        {
            NotificationManager.Instance?.ShowNotification("Xung đột dữ liệu! Vui lòng tải lại.", 3f);
        }

        return result;
    }

    /// <summary>
    /// Performs a granular update — writes only specific fields instead of full document.
    /// Prevents overwriting concurrent changes to other fields.
    /// </summary>
    /// <param name="path">Base path in Firebase.</param>
    /// <param name="updates">Dictionary of child-path → value pairs to update.</param>
    public async Task<FirebaseResponse<bool>> UpdateFields(string path, Dictionary<string, object> updates, int maxRetries = 3)
    {
        float delay = InitialDelay;

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var task = reference.Child(path).UpdateChildrenAsync(updates);
                await task;

                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"FirebaseTransaction: Granular update succeeded at {path} ({updates.Count} fields)");
                    return FirebaseResponse<bool>.Ok(true);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"FirebaseTransaction: Granular update failed at {path} (attempt {attempt + 1}/{maxRetries}): {ex.Message}");
            }

            if (attempt < maxRetries - 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(delay));
                delay = Mathf.Min(delay * BackoffMultiplier, MaxDelay);
            }
        }

        return FirebaseResponse<bool>.Fail($"Granular update failed after {maxRetries} retries");
    }

    /// <summary>
    /// Legacy RunTransaction (read-then-write) — kept for backward compatibility.
    /// Prefer RunAtomicTransaction for true concurrency safety.
    /// </summary>
    public async Task<FirebaseResponse<bool>> RunTransaction(string path, Func<string, string> updateFunc, int maxRetries = 3)
    {
        return await RunAtomicTransaction(path, updateFunc, maxRetries);
    }

    /// <summary>
    /// Converts a deserialized object to Firebase-compatible value types.
    /// Firebase SDK expects Dictionary, List, or primitive types.
    /// </summary>
    private object ConvertToFirebaseValue(object obj)
    {
        if (obj == null) return null;

        // Newtonsoft deserializes to JObject/JArray — convert to Dictionary/List
        if (obj is Newtonsoft.Json.Linq.JObject jObj)
        {
            return jObj.ToObject<Dictionary<string, object>>();
        }

        if (obj is Newtonsoft.Json.Linq.JArray jArr)
        {
            return jArr.ToObject<System.Collections.Generic.List<object>>();
        }

        return obj;
    }

    /// <summary>
    /// Converts a Firebase value to the specified type.
    /// </summary>
    private T ConvertValue<T>(object value)
    {
        if (value == null)
        {
            return default(T);
        }

        if (value is string stringValue)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)FirebaseJsonUtility.NormalizeReadValue(stringValue);
            }

            return JsonConvert.DeserializeObject<T>(FirebaseJsonUtility.NormalizeReadValue(stringValue));
        }

        if (typeof(T) == typeof(string))
        {
            return (T)(object)JsonConvert.SerializeObject(value);
        }

        Type targetType = typeof(T);

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            targetType = Nullable.GetUnderlyingType(targetType);
        }

        try
        {
            return (T)Convert.ChangeType(value, targetType);
        }
        catch
        {
            if (targetType == typeof(string))
            {
                return (T)(object)value.ToString();
            }
            throw;
        }
    }
}
