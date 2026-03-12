using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Yarn.Unity;

/// <summary>
/// Manages main menu functionality including name entry, game start/continue,
/// settings configuration, and scene transitions.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Name Entry")]
    [Tooltip("Input field for player name")]
    [SerializeField] private TMP_InputField nameInputField;
    
    [Tooltip("Yarn Spinner variable storage for saving player name")]
    [SerializeField] private VariableStorageBehaviour variableStorage;
    
    [Header("Audio")]
    [Tooltip("Background music for main menu")]
    [SerializeField] private AudioSource menuMusic;
    
    [Header("Scene Settings")]
    [Tooltip("Name of the main game scene")]
    [SerializeField] private string gameSceneName = "CoffeeShop";
    
    [Header("Default Settings")]
    [Tooltip("Default mouse sensitivity")]
    [SerializeField] private float defaultSensitivity = 2f;
    
    [Tooltip("Default audio volume (0-1)")]
    [SerializeField] private float defaultVolume = 1f;
    
    #endregion
    
    #region Private Fields
    
    // Constants
    private const string YARN_PLAYER_NAME_VARIABLE = "$PlayerName";
    private const string LAST_SCENE_PREF_KEY = "LastScene";
    private const string SENSITIVITY_PREF_KEY = "MouseSensitivity";
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeMenu();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeMenu()
    {
        SetupCursor();
        StartMenuMusic();
        LoadSettings();
    }
    
    private void SetupCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void StartMenuMusic()
    {
        if (menuMusic != null && !menuMusic.isPlaying)
        {
            menuMusic.Play();
        }
    }
    
    private void LoadSettings()
    {
        // Apply saved settings
        float savedSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_PREF_KEY, defaultSensitivity);
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", defaultVolume);
    }
    
    #endregion
    
    #region Game Flow - Start/Continue
    
    /// <summary>
    /// Starts a new game with the entered player name.
    /// Called by New Game button.
    /// </summary>
    public void StartNewGame()
    {
        if (!ValidatePlayerName())
        {
            ShowNameRequiredWarning();
            return;
        }
        
        SavePlayerName();
        LoadGameScene();
    }
    
    /// <summary>
    /// Continues from the last saved scene, or starts new game if no save exists.
    /// Called by Continue button.
    /// </summary>
    public void ContinueGame()
    {
        if (HasSavedGame())
        {
            LoadSavedGame();
        }
        else
        {
            Debug.Log("[MainMenuManager] No save found, starting new game");
            StartNewGame();
        }
    }
    
    /// <summary>
    /// Exits the application.
    /// Called by Exit button.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("[MainMenuManager] Exiting game");
        
        #if UNITY_EDITOR
        QuitEditor();
        #else
        QuitApplication();
        #endif
    }
    
    #endregion
    
    #region Player Name Management
    
    private bool ValidatePlayerName()
    {
        if (nameInputField == null)
        {
            Debug.LogError("[MainMenuManager] Name input field not assigned!");
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            return false;
        }
        
        return true;
    }
    
    private void ShowNameRequiredWarning()
    {
        Debug.LogWarning("[MainMenuManager] Player must enter a name first!");
        
        // Optional: Show UI feedback
        // FeedbackManager.Instance?.ShowError("Please enter your name");
    }
    
    private void SavePlayerName()
    {
        string playerName = nameInputField.text.Trim();
        
        if (variableStorage != null)
        {
            try
            {
                variableStorage.SetValue(YARN_PLAYER_NAME_VARIABLE, playerName);
                Debug.Log($"[MainMenuManager] Saved player name: {playerName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainMenuManager] Failed to save player name: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Variable storage not assigned. Name won't persist in dialogues.");
        }
    }
    
    #endregion
    
    #region Save System
    
    private bool HasSavedGame()
    {
        return PlayerPrefs.HasKey(LAST_SCENE_PREF_KEY);
    }
    
    private void LoadSavedGame()
    {
        string savedScene = PlayerPrefs.GetString(LAST_SCENE_PREF_KEY);
        
        if (string.IsNullOrEmpty(savedScene))
        {
            Debug.LogWarning("[MainMenuManager] Saved scene name is empty");
            StartNewGame();
            return;
        }
        
        Debug.Log($"[MainMenuManager] Loading saved game: {savedScene}");
        SceneManager.LoadScene(savedScene);
    }
    
    /// <summary>
    /// Saves the current scene as the last played scene.
    /// Should be called from gameplay scenes (not main menu).
    /// </summary>
    public static void SaveCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Don't save the main menu itself
        if (currentScene == MAIN_MENU_SCENE_NAME)
        {
            return;
        }
        
        PlayerPrefs.SetString(LAST_SCENE_PREF_KEY, currentScene);
        PlayerPrefs.Save();
        
        Debug.Log($"[MainMenuManager] Saved current scene: {currentScene}");
    }
    
    /// <summary>
    /// Clears the saved game data.
    /// </summary>
    public void ClearSaveData()
    {
        if (PlayerPrefs.HasKey(LAST_SCENE_PREF_KEY))
        {
            PlayerPrefs.DeleteKey(LAST_SCENE_PREF_KEY);
            PlayerPrefs.Save();
            Debug.Log("[MainMenuManager] Save data cleared");
        }
    }
    
    #endregion
    
    #region Scene Loading
    
    private void LoadGameScene()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenuManager] Game scene name not set!");
            return;
        }
        
        Debug.Log($"[MainMenuManager] Loading game scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName);
    }
    
    #endregion
    
    #region Settings - Audio
    
    /// <summary>
    /// Sets the master volume.
    /// Called by volume slider.
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        AudioListener.volume = clampedVolume;
        
        // Save preference
        PlayerPrefs.SetFloat("Volume", clampedVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"[MainMenuManager] Volume set to: {clampedVolume:F2}");
    }
    
    #endregion
    
    #region Settings - Controls
    
    /// <summary>
    /// Sets mouse sensitivity and saves to PlayerPrefs.
    /// Called by sensitivity slider.
    /// </summary>
    /// <param name="sensitivity">Sensitivity value</param>
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, sensitivity);
        PlayerPrefs.Save();
        
        Debug.Log($"[MainMenuManager] Sensitivity set to: {sensitivity:F2}");
    }
    
    #endregion
    
    #region Settings - Graphics
    
    /// <summary>
    /// Sets screen resolution.
    /// Called by resolution dropdown.
    /// </summary>
    /// <param name="resolutionIndex">Index of resolution preset</param>
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = GetResolutionFromIndex(resolutionIndex);
        
        if (resolution.width > 0)
        {
            Screen.SetResolution(
                resolution.width,
                resolution.height,
                FullScreenMode.FullScreenWindow
            );
            
            Debug.Log($"[MainMenuManager] Resolution changed to {resolution.width}x{resolution.height}");
        }
        else
        {
            Debug.LogWarning($"[MainMenuManager] Invalid resolution index: {resolutionIndex}");
        }
    }
    
    private Resolution GetResolutionFromIndex(int index)
    {
        Resolution res = new Resolution();
        
        switch (index)
        {
            case 0: // 1080p
                res.width = 1920;
                res.height = 1080;
                break;
            
            case 1: // 720p
                res.width = 1280;
                res.height = 720;
                break;
            
            case 2: // Steam Deck native
                res.width = 1280;
                res.height = 800;
                break;
            
            case 3: // 1440p
                res.width = 2560;
                res.height = 1440;
                break;
            
            case 4: // 4K
                res.width = 3840;
                res.height = 2160;
                break;
            
            default:
                Debug.LogWarning($"[MainMenuManager] Unknown resolution index: {index}");
                break;
        }
        
        return res;
    }
    
    #endregion
    
    #region Application Control
    
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
    
    #region Public Query Methods
    
    /// <summary>
    /// Checks if a saved game exists.
    /// </summary>
    public bool DoesSaveExist()
    {
        return HasSavedGame();
    }
    
    /// <summary>
    /// Gets the name of the saved scene (if it exists).
    /// </summary>
    public string GetSavedSceneName()
    {
        return HasSavedGame() ? PlayerPrefs.GetString(LAST_SCENE_PREF_KEY) : string.Empty;
    }
    
    #endregion
}

/// <summary>
/// Helper component that automatically saves the current scene when it loads.
/// Attach to a GameObject in gameplay scenes (not main menu).
/// </summary>
public class SceneSaveHelper : MonoBehaviour
{
    #region Unity Lifecycle
    
    private void Start()
    {
        SaveCurrentScene();
    }
    
    #endregion
    
    #region Save Logic
    
    private void SaveCurrentScene()
    {
        MainMenuManager.SaveCurrentScene();
    }
    
    #endregion
}