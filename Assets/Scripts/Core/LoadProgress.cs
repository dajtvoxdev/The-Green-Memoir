using UnityEngine;

/// <summary>
/// Represents the progress of an async loading operation.
/// Phase 1 Feature (#29): Async loading with progress tracking.
/// </summary>
public struct LoadProgress
{
    /// <summary>
    /// Normalized progress value from 0.0 to 1.0.
    /// </summary>
    public float normalized;
    
    /// <summary>
    /// Human-readable status message (e.g., "Loading map...", "Fetching user data...").
    /// </summary>
    public string statusMessage;
    
    /// <summary>
    /// True when loading is complete (normalized >= 1.0).
    /// </summary>
    public bool isComplete;
    
    /// <summary>
    /// Creates a new LoadProgress instance.
    /// </summary>
    /// <param name="normalized">Progress value (0.0 to 1.0)</param>
    /// <param name="status">Status message</param>
    public LoadProgress(float normalized, string status)
    {
        this.normalized = Mathf.Clamp01(normalized);
        this.statusMessage = status;
        this.isComplete = this.normalized >= 1.0f;
    }
    
    /// <summary>
    /// Returns a LoadProgress representing the start of loading.
    /// </summary>
    public static LoadProgress Start(string status = "Starting...")
    {
        return new LoadProgress(0f, status);
    }
    
    /// <summary>
    /// Returns a LoadProgress representing completion.
    /// </summary>
    public static LoadProgress Complete(string status = "Complete!")
    {
        return new LoadProgress(1f, status);
    }
    
    /// <summary>
    /// Creates a new progress with updated status message.
    /// </summary>
    public LoadProgress WithStatus(string newStatus)
    {
        return new LoadProgress(normalized, newStatus);
    }
    
    /// <summary>
    /// Creates a new progress with updated normalized value.
    /// </summary>
    public LoadProgress WithProgress(float newProgress)
    {
        return new LoadProgress(newProgress, statusMessage);
    }
}