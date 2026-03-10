using System;
using Firebase; // Core Firebase SDK – khởi tạo app Firebase trong Unity
using Firebase.Database; // Firebase Realtime Database API
using Firebase.Extensions; // Cung cấp ContinueWithOnMainThread. Giúp callback chạy trên MAIN THREAD của Unity
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
        reference.Child(path).SetValueAsync(data).ContinueWithOnMainThread(task =>
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
                string value = snapshot.Value?.ToString();
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
