using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages asynchronous scene loading with progress tracking.
/// Phase 1 Feature (#29): Fixes race condition by waiting for Firebase data before scene transition.
/// </summary>
public class AsyncLoadingManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static AsyncLoadingManager Instance { get; private set; }
    
    [Header("UI References")]
    [Tooltip("The progress bar slider")]
    public Slider progressBar;
    
    [Tooltip("The text displaying progress percentage")]
    public Text progressText;
    
    [Tooltip("The text displaying status message")]
    public Text statusText;
    
    [Header("Loading Settings")]
    [Tooltip("The scene to load after data is ready")]
    public string targetSceneName;
    
    [Tooltip("Minimum time to show loading screen (prevents flickering)")]
    public float minLoadingTime = 1f;
    
    /// <summary>
    /// Current loading progress.
    /// </summary>
    public LoadProgress CurrentProgress { get; private set; }
    
    /// <summary>
    /// Event fired when progress updates.
    /// </summary>
    public event Action<LoadProgress> OnProgressUpdated;
    
    /// <summary>
    /// Event fired when loading completes.
    /// </summary>
    public event Action OnLoadingComplete;
    
    private bool isDataReady = false;
    private float loadingStartTime;
    private AsyncOperation sceneAsyncOperation;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        loadingStartTime = Time.time;
        CurrentProgress = LoadProgress.Start(LocalizationManager.LocalizeText("Initializing..."));
        UpdateUI();
    }
    
    /// <summary>
    /// Starts the async loading process.
    /// Call this after initiating Firebase data fetch.
    /// </summary>
    public void StartLoading(string sceneName)
    {
        targetSceneName = sceneName;
        CurrentProgress = LoadProgress.Start(LocalizationManager.LocalizeText("Connecting to server..."));
        UpdateUI();
    }
    
    /// <summary>
    /// Call this when Firebase data has been successfully loaded.
    /// </summary>
    public void OnDataReady()
    {
        isDataReady = true;
        CurrentProgress = new LoadProgress(0.5f, LocalizationManager.LocalizeText("Data loaded. Preparing scene..."));
        UpdateUI();
        
        // Start loading the scene
        StartCoroutine(LoadSceneAsync());
    }
    
    /// <summary>
    /// Call this when Firebase data loading fails.
    /// </summary>
    public void OnDataFailed(string errorMessage)
    {
        CurrentProgress = new LoadProgress(0f, LocalizationManager.LocalizeText($"Error: {errorMessage}"));
        UpdateUI();
        Debug.LogError($"AsyncLoadingManager: Data load failed - {errorMessage}");
    }
    
    /// <summary>
    /// Updates the progress value and status message.
    /// </summary>
    public void UpdateProgress(float normalized, string status)
    {
        CurrentProgress = new LoadProgress(normalized, status);
        UpdateUI();
    }
    
    /// <summary>
    /// Updates the UI elements with current progress.
    /// </summary>
    private void UpdateUI()
    {
        if (progressBar != null)
        {
            progressBar.value = CurrentProgress.normalized;
        }
        
        if (progressText != null)
        {
            progressText.text = $"{(CurrentProgress.normalized * 100):F0}%";
        }
        
        if (statusText != null)
        {
            statusText.text = CurrentProgress.statusMessage;
        }
        
        OnProgressUpdated?.Invoke(CurrentProgress);
    }
    
    /// <summary>
    /// Coroutine that handles async scene loading.
    /// </summary>
    private IEnumerator LoadSceneAsync()
    {
        // Guard: ensure data is loaded before scene transition
        if (!isDataReady)
        {
            Debug.LogWarning("AsyncLoadingManager: Data not ready, waiting...");
            yield return new WaitUntil(() => isDataReady);
        }

        // Ensure minimum loading time has passed
        float elapsed = Time.time - loadingStartTime;
        if (elapsed < minLoadingTime)
        {
            yield return new WaitForSeconds(minLoadingTime - elapsed);
        }
        
        // Update progress to 75% before scene load
        CurrentProgress = new LoadProgress(0.75f, LocalizationManager.LocalizeText("Loading scene..."));
        UpdateUI();
        
        // Start async scene load
        sceneAsyncOperation = SceneManager.LoadSceneAsync(targetSceneName);
        sceneAsyncOperation.allowSceneActivation = false;
        
        // Wait until scene is fully loaded
        while (!sceneAsyncOperation.isDone)
        {
            // Scene load progress goes from 0 to 0.9, then waits for allowSceneActivation
            float sceneProgress = sceneAsyncOperation.progress;
            
            // Map scene progress from [0, 0.9] to [0.75, 0.95]
            float mappedProgress = 0.75f + (sceneProgress / 0.9f) * 0.2f;
            CurrentProgress = new LoadProgress(mappedProgress, LocalizationManager.LocalizeText("Loading scene..."));
            UpdateUI();
            
            // When scene is fully loaded (reaches 0.95)
            if (sceneProgress >= 0.9f)
            {
                // Small delay for visual feedback
                yield return new WaitForSeconds(0.3f);
                
                // Activate the scene
                sceneAsyncOperation.allowSceneActivation = true;
                
                // Mark as complete
                CurrentProgress = LoadProgress.Complete(LocalizationManager.LocalizeText("Loading complete!"));
                UpdateUI();
                
                // Fire completion event
                OnLoadingComplete?.Invoke();
                
                // Wait for scene to actually activate
                while (!sceneAsyncOperation.isDone)
                {
                    yield return null;
                }
                
                // Clean up this manager after scene transition
                Destroy(gameObject);
                yield break;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Loads a scene asynchronously with progress tracking.
    /// Use this for simple scene loads without Firebase dependency.
    /// </summary>
    public void LoadSceneAsync(string sceneName)
    {
        targetSceneName = sceneName;
        isDataReady = true;
        CurrentProgress = LoadProgress.Complete(LocalizationManager.LocalizeText("Ready"));
        StartCoroutine(LoadSceneAsync());
    }
    
    /// <summary>
    /// Waits for Firebase data before proceeding with scene load.
    /// This is the main entry point for fixing the race condition (T3).
    /// </summary>
    public static IEnumerator WaitForData(Func<bool> dataCheckFunc, string sceneName, Action<LoadProgress> progressCallback = null)
    {
        if (Instance == null)
        {
            Debug.LogError("AsyncLoadingManager: Instance not found!");
            yield break;
        }
        
        Instance.StartLoading(sceneName);
        
        // Wait for data to be ready
        while (!dataCheckFunc())
        {
            Instance.UpdateProgress(0.25f, LocalizationManager.LocalizeText("Fetching user data from server..."));
            yield return new WaitForSeconds(0.1f);
        }
        
        // Data is ready, proceed with loading
        Instance.OnDataReady();
    }
}
