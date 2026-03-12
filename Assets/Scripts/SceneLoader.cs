using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Persistent scene loader that survives scene transitions.
/// Provides various methods for loading, reloading, and transitioning between scenes.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    #region Singleton
    
    public static SceneLoader Instance { get; private set; }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("Loading Settings")]
    [Tooltip("Default delay before loading scenes (seconds)")]
    [SerializeField] private float defaultLoadDelay = 0f;
    
    [Header("Debug")]
    [Tooltip("Enable debug logging for scene transitions")]
    [SerializeField] private bool enableDebugLogs = false;
    
    #endregion
    
    #region Private Fields
    
    private bool isLoadingScene = false;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LogDebug("[SceneLoader] Initialized and set to persist across scenes");
    }
    
    #endregion
    
    #region Scene Loading - By Name
    
    /// <summary>
    /// Loads a scene by name immediately.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        if (!ValidateSceneName(sceneName))
        {
            return;
        }
        
        LogDebug($"[SceneLoader] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Loads a scene by name after a delay.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    /// <param name="delay">Delay in seconds before loading</param>
    public void LoadSceneWithDelay(string sceneName, float delay = -1f)
    {
        if (!ValidateSceneName(sceneName))
        {
            return;
        }
        
        float actualDelay = delay >= 0f ? delay : defaultLoadDelay;
        
        if (isLoadingScene)
        {
            Debug.LogWarning("[SceneLoader] Scene load already in progress");
            return;
        }
        
        StartCoroutine(LoadSceneDelayedCoroutine(sceneName, actualDelay));
    }
    
    #endregion
    
    #region Scene Loading - By Index
    
    /// <summary>
    /// Loads a scene by build index immediately.
    /// </summary>
    /// <param name="sceneIndex">Build index of the scene</param>
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (!ValidateSceneIndex(sceneIndex))
        {
            return;
        }
        
        LogDebug($"[SceneLoader] Loading scene at index: {sceneIndex}");
        SceneManager.LoadScene(sceneIndex);
    }
    
    /// <summary>
    /// Loads the next scene in the build order.
    /// </summary>
    public void LoadNextScene()
    {
        int nextSceneIndex = GetNextSceneIndex();
        
        if (ValidateSceneIndex(nextSceneIndex))
        {
            LogDebug($"[SceneLoader] Loading next scene: {nextSceneIndex}");
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("[SceneLoader] No next scene available in build settings");
        }
    }
    
    /// <summary>
    /// Loads the previous scene in the build order.
    /// </summary>
    public void LoadPreviousScene()
    {
        int previousSceneIndex = GetPreviousSceneIndex();
        
        if (ValidateSceneIndex(previousSceneIndex))
        {
            LogDebug($"[SceneLoader] Loading previous scene: {previousSceneIndex}");
            SceneManager.LoadScene(previousSceneIndex);
        }
        else
        {
            Debug.LogWarning("[SceneLoader] No previous scene available in build settings");
        }
    }
    
    #endregion
    
    #region Scene Reloading
    
    /// <summary>
    /// Reloads the current scene (useful for restarts/retries).
    /// </summary>
    public void ReloadScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        LogDebug($"[SceneLoader] Reloading current scene: {currentSceneName}");
        SceneManager.LoadScene(currentSceneName);
    }
    
    /// <summary>
    /// Reloads the current scene after a delay.
    /// </summary>
    /// <param name="delay">Delay in seconds before reloading</param>
    public void ReloadSceneWithDelay(float delay)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadSceneDelayedCoroutine(currentSceneName, delay));
    }
    
    #endregion
    
    #region Application Control
    
    /// <summary>
    /// Quits the application (or stops play mode in editor).
    /// </summary>
    public void QuitGame()
    {
        LogDebug("[SceneLoader] Quitting game");
        
        #if UNITY_EDITOR
        QuitEditor();
        #else
        QuitApplication();
        #endif
    }
    
    private void QuitApplication()
    {
        Application.Quit();
    }
    
    #if UNITY_EDITOR
    private void QuitEditor()
    {
        UnityEditor.EditorApplication.isPlaying = false;
    }
    #endif
    
    #endregion
    
    #region Validation
    
    private bool ValidateSceneName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[SceneLoader] Scene name is null or empty");
            return false;
        }
        
        // Check if scene exists in build settings
        if (SceneManager.GetSceneByName(sceneName).buildIndex == -1)
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' not found in build settings");
            return false;
        }
        
        return true;
    }
    
    private bool ValidateSceneIndex(int sceneIndex)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        
        if (sceneIndex < 0 || sceneIndex >= sceneCount)
        {
            Debug.LogError($"[SceneLoader] Scene index {sceneIndex} is out of range (0-{sceneCount - 1})");
            return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Helper Methods
    
    private int GetNextSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex + 1;
    }
    
    private int GetPreviousSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex - 1;
    }
    
    #endregion
    
    #region Coroutines
    
    private IEnumerator LoadSceneDelayedCoroutine(string sceneName, float delay)
    {
        isLoadingScene = true;
        
        LogDebug($"[SceneLoader] Waiting {delay:F1}s before loading: {sceneName}");
        
        yield return new WaitForSeconds(delay);
        
        LogDebug($"[SceneLoader] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
        
        isLoadingScene = false;
    }
    
    #endregion
    
    #region Public Query Methods
    
    /// <summary>
    /// Returns the name of the currently active scene.
    /// </summary>
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    /// <summary>
    /// Returns the build index of the currently active scene.
    /// </summary>
    public int GetCurrentSceneBuildIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
    
    /// <summary>
    /// Checks if a scene load is currently in progress.
    /// </summary>
    public bool IsLoadingScene()
    {
        return isLoadingScene;
    }
    
    /// <summary>
    /// Returns the total number of scenes in build settings.
    /// </summary>
    public int GetTotalSceneCount()
    {
        return SceneManager.sceneCountInBuildSettings;
    }
    
    #endregion
    
    #region Debug Logging
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    #endregion
}