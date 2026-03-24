using System;
using Firebase; // Core Firebase SDK – khởi tạo app Firebase trong Unity
using Firebase.Database; // Firebase Realtime Database API
using Firebase.Extensions; // Cung cấp ContinueWithOnMainThread. Giúp callback chạy trên MAIN THREAD của Unity
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Manager for Firebase Realtime Database operations.
/// Provides async methods with callbacks for read/write operations.
/// </summary>
public class FirebaseDatabaseManager : MonoBehaviour
{
    private DatabaseReference reference;
    
    /// <summary>
    /// Singleton instance for easy access.
    /// </summary>
    public static FirebaseDatabaseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            FirebaseApp app = FirebaseApp.DefaultInstance;
            reference = FirebaseDatabase.DefaultInstance.RootReference;
            EnsureTransactionManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Writes data to Firebase at the specified path.
    /// </summary>
    /// <param name="path">The Firebase path (e.g., "Users/userId/map/tiles/3_5")</param>
    /// <param name="data">The data to write (will be serialized to JSON)</param>
    /// <param name="onComplete">Callback with success status and error message</param>
    public void WriteDatabase(string path, string data, Action<bool, string> onComplete = null)
    {
        if (IsUserProfilePath(path))
        {
            WriteUserProfileSafely(path, data, onComplete);
            return;
        }

        WriteRawJson(path, data, onComplete);
    }

    private void EnsureTransactionManager()
    {
        if (GetComponent<FirebaseTransactionManager>() == null)
        {
            gameObject.AddComponent<FirebaseTransactionManager>();
            Debug.Log("FirebaseDatabaseManager: Added missing FirebaseTransactionManager at runtime.");
        }
    }

    private bool IsUserProfilePath(string path)
    {
        return !string.IsNullOrWhiteSpace(path)
            && path.Replace('\\', '/').EndsWith("/profile", StringComparison.OrdinalIgnoreCase);
    }

    private void WriteUserProfileSafely(string path, string data, Action<bool, string> onComplete)
    {
        reference.Child(path).GetValueAsync().ContinueWithOnMainThread(readTask =>
        {
            if (readTask.IsFaulted)
            {
                string errorMessage = readTask.Exception?.GetBaseException().Message ?? "Unknown read error";
                Debug.LogWarning($"Firebase: Failed to read existing profile before write at path {path}: {errorMessage}");
                WriteRawJson(path, data, onComplete);
                return;
            }

            string currentJson = readTask.Result?.Value == null
                ? null
                : FirebaseJsonUtility.NormalizeReadValue(readTask.Result.GetRawJsonValue());

            string mergedJson = MergeProfileJson(currentJson, data);
            WriteRawJson(path, mergedJson, onComplete);
        });
    }

    private void WriteRawJson(string path, string data, Action<bool, string> onComplete)
    {
        string jsonPayload = FirebaseJsonUtility.PrepareForWrite(data);

        reference.Child(path).SetRawJsonValueAsync(jsonPayload).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"Firebase: Write success at path: {path}");
                onComplete?.Invoke(true, null);
            }
            else
            {
                string errorMessage = task.Exception?.GetBaseException().Message ?? "Unknown error";
                Debug.LogError($"Firebase: Write failed at path {path}: {errorMessage}");
                onComplete?.Invoke(false, errorMessage);
            }
        });
    }

    private string MergeProfileJson(string currentJson, string incomingJson)
    {
        string normalizedIncoming = FirebaseJsonUtility.NormalizeReadValue(incomingJson);
        if (string.IsNullOrWhiteSpace(normalizedIncoming))
        {
            return incomingJson;
        }

        try
        {
            if (JToken.Parse(normalizedIncoming) is not JObject incomingObj)
            {
                return incomingJson;
            }

            JObject merged = (JObject)incomingObj.DeepClone();
            JObject currentObj = null;

            if (!string.IsNullOrWhiteSpace(currentJson))
            {
                string normalizedCurrent = FirebaseJsonUtility.NormalizeReadValue(currentJson);
                if (!string.IsNullOrWhiteSpace(normalizedCurrent)
                    && JToken.Parse(normalizedCurrent) is JObject parsedCurrent)
                {
                    currentObj = parsedCurrent;
                }
            }

            if (!HasUsableMap(merged) && HasUsableMap(currentObj))
            {
                merged["MapInGame"] = currentObj["MapInGame"]?.DeepClone();
                Debug.LogWarning("FirebaseDatabaseManager: Preserved server MapInGame while writing /profile.");
            }

            if (!ReadBool(merged["hasPurchased"]) && ReadBool(currentObj?["hasPurchased"]))
            {
                merged["hasPurchased"] = true;
            }

            long currentVersion = ReadLong(currentObj?["Version"]);
            long incomingVersion = ReadLong(merged["Version"]);
            if (currentVersion > 0 && incomingVersion <= 0)
            {
                merged["Version"] = currentVersion;
            }

            return merged.ToString(Newtonsoft.Json.Formatting.None);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"FirebaseDatabaseManager: Failed to merge profile JSON safely: {ex.Message}");
            return incomingJson;
        }
    }

    private bool HasUsableMap(JObject profileObj)
    {
        return profileObj?["MapInGame"]?["lstTilemapDetail"] != null;
    }

    private bool ReadBool(JToken token)
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

            string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString());
            return bool.TryParse(normalized, out bool parsed) && parsed;
        }
        catch
        {
            return false;
        }
    }

    private long ReadLong(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return 0;
        }

        try
        {
            if (token.Type == JTokenType.Integer)
            {
                return token.Value<long>();
            }

            string normalized = FirebaseJsonUtility.NormalizeReadValue(token.ToString());
            return long.TryParse(normalized, out long parsed) ? parsed : 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Reads data from Firebase at the specified path.
    /// </summary>
    /// <param name="path">The Firebase path to read from</param>
    /// <param name="onComplete">Callback with the data string (null if failed)</param>
    public void ReadDatabase(string path, Action<string> onComplete = null)
    {
        reference.Child(path).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result != null)
            {
                DataSnapshot snapshot = task.Result;
                string value = snapshot.Value == null
                    ? null
                    : FirebaseJsonUtility.NormalizeReadValue(snapshot.GetRawJsonValue());
                Debug.Log($"Firebase: Read success from path: {path}");
                onComplete?.Invoke(value);
            }
            else
            {
                string errorMessage = task.Exception?.GetBaseException().Message ?? "Unknown error";
                Debug.LogError($"Firebase: Read failed from path {path}: {errorMessage}");
                onComplete?.Invoke(null);
            }
        });
    }
    
    /// <summary>
    /// Gets a DatabaseReference for the specified path.
    /// Useful for direct operations without callbacks.
    /// </summary>
    /// <param name="path">The path to get reference for</param>
    /// <returns>The DatabaseReference</returns>
    public DatabaseReference GetReference(string path)
    {
        return reference.Child(path);
    }
}
