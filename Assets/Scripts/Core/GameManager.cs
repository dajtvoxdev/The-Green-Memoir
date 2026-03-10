using UnityEngine;

/// <summary>
/// Singleton GameManager for global state management.
/// Phase 1: Provides centralized game state control.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance.
    /// </summary>
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [Tooltip("Whether the game is currently paused")]
    public bool isPaused = false;
    
    [Tooltip("Whether the game is in loading state")]
    public bool isLoading = false;
    
    /// <summary>
    /// Current game state enum.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        Saving
    }
    
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    
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
        Initialize();
    }
    
    /// <summary>
    /// Initialize the game manager.
    /// Called once at game start.
    /// </summary>
    public void Initialize()
    {
        Debug.Log("GameManager: Initialized");
        CurrentState = GameState.MainMenu;
    }
    
    /// <summary>
    /// Sets the current game state.
    /// </summary>
    /// <param name="newState">The new state</param>
    public void SetGameState(GameState newState)
    {
        GameState oldState = CurrentState;
        CurrentState = newState;
        
        Debug.Log($"GameManager: State changed from {oldState} to {newState}");
        
        // Handle state-specific logic
        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
            case GameState.Playing:
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;
        }
    }
    
    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void Pause()
    {
        if (CurrentState == GameState.Playing)
        {
            isPaused = true;
            SetGameState(GameState.Paused);
        }
    }
    
    /// <summary>
    /// Resumes the game from pause.
    /// </summary>
    public void Resume()
    {
        if (CurrentState == GameState.Paused)
        {
            isPaused = false;
            SetGameState(GameState.Playing);
        }
    }
    
    /// <summary>
    /// Toggles pause state.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }
    
    /// <summary>
    /// Sets loading state.
    /// </summary>
    /// <param name="loading">True if loading</param>
    public void SetLoading(bool loading)
    {
        isLoading = loading;
        if (loading)
        {
            SetGameState(GameState.Loading);
        }
    }
    
    /// <summary>
    /// Sets the game to playing state.
    /// Called after loading completes.
    /// </summary>
    public void StartPlaying()
    {
        SetGameState(GameState.Playing);
    }
    
    /// <summary>
    /// Sets the game to saving state.
    /// </summary>
    public void StartSaving()
    {
        SetGameState(GameState.Saving);
    }
    
    /// <summary>
    /// Resets game state (for logout or restart).
    /// </summary>
    public void Reset()
    {
        isPaused = false;
        isLoading = false;
        CurrentState = GameState.MainMenu;
        Time.timeScale = 1f;
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}