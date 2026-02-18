using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages pause menu state, navigation between submenus, and game settings.
/// Handles pause/resume, settings adjustments, and menu navigation with ESC key.
/// </summary>
public class PauseManager : MonoBehaviour
{
    #region Singleton
    
    public static PauseManager Instance { get; private set; }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("Menu UI")]
    [Tooltip("Parent canvas containing all pause menu elements")]
    [SerializeField] private GameObject pauseMenuCanvas;
    
    [Tooltip("Main pause menu panel")]
    [SerializeField] private GameObject pausePanel;
    
    [Tooltip("Settings submenu panel")]
    [SerializeField] private GameObject settingsPanel;
    
    [Tooltip("Controls submenu panel")]
    [SerializeField] private GameObject controlsPanel;
    
    [Header("Scene Settings")]
    [Tooltip("Name of the main menu scene to load")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("Default Settings")]
    [Tooltip("Default mouse sensitivity")]
    [SerializeField] private float defaultSensitivity = 2f;
    
    [Tooltip("Default audio volume (0-1)")]
    [SerializeField] private float defaultVolume = 1f;
    
    #endregion
    
    #region Private Fields
    
    private bool isPaused = false;
    private bool isInSettings = false;
    private bool isInControls = false;
    
    // Constants
    private const KeyCode PAUSE_KEY = KeyCode.Escape;
    private const string SENSITIVITY_PREF_KEY = "MouseSensitivity";
    private const float PAUSED_TIME_SCALE = 0f;
    private const float NORMAL_TIME_SCALE = 1f;
    
    #endregion
    
    #region Menu State Enum
    
    private enum MenuState
    {
        Playing,
        MainPause,
        Settings,
        Controls
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeMenus();
        LoadSettings();
    }
    
    private void Update()
    {
        HandlePauseInput();
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
    }
    
    private void InitializeMenus()
    {
        HideAllMenus();
    }
    
    private void HideAllMenus()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        if (controlsPanel != null)
        {
            controlsPanel.SetActive(false);
        }
    }
    
    private void LoadSettings()
    {
        // Load saved sensitivity or use default
        float savedSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_PREF_KEY, defaultSensitivity);
        AudioListener.volume = defaultVolume;
    }
    
    #endregion
    
    #region Input Handling
    
    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(PAUSE_KEY))
        {
            NavigateBack();
        }
    }
    
    private void NavigateBack()
    {
        MenuState currentState = GetCurrentMenuState();
        
        switch (currentState)
        {
            case MenuState.Controls:
                CloseControls();
                break;
            
            case MenuState.Settings:
                CloseSettings();
                break;
            
            case MenuState.MainPause:
                Resume();
                break;
            
            case MenuState.Playing:
                Pause();
                break;
        }
    }
    
    private MenuState GetCurrentMenuState()
    {
        if (isInControls) return MenuState.Controls;
        if (isInSettings) return MenuState.Settings;
        if (isPaused) return MenuState.MainPause;
        return MenuState.Playing;
    }
    
    #endregion
    
    #region Pause/Resume
    
    /// <summary>
    /// Pauses the game and shows the pause menu.
    /// </summary>
    public void Pause()
    {
        isPaused = true;
        
        ShowPauseMenu();
        FreezeGame();
        ShowCursor();
        
        Debug.Log("[PauseManager] Game paused");
    }
    
    /// <summary>
    /// Resumes the game and hides all menus.
    /// </summary>
    public void Resume()
    {
        ResetMenuStates();
        HideAllMenus();
        UnfreezeGame();
        HideCursor();
        
        Debug.Log("[PauseManager] Game resumed");
    }
    
    private void ShowPauseMenu()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    private void ResetMenuStates()
    {
        isPaused = false;
        isInSettings = false;
        isInControls = false;
    }
    
    private void FreezeGame()
    {
        Time.timeScale = PAUSED_TIME_SCALE;
    }
    
    private void UnfreezeGame()
    {
        Time.timeScale = NORMAL_TIME_SCALE;
    }
    
    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    private void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    #endregion
    
    #region Menu Navigation
    
    /// <summary>
    /// Opens the settings submenu. Called by Settings button.
    /// </summary>
    public void OpenSettings()
    {
        isInSettings = true;
        
        HidePanel(pausePanel);
        ShowPanel(settingsPanel);
    }
    
    /// <summary>
    /// Closes settings and returns to main pause menu.
    /// </summary>
    public void CloseSettings()
    {
        isInSettings = false;
        isInControls = false;
        
        HidePanel(settingsPanel);
        HidePanel(controlsPanel);
        ShowPanel(pausePanel);
    }
    
    /// <summary>
    /// Opens the controls submenu. Called by Controls button in settings.
    /// </summary>
    public void OpenControls()
    {
        isInControls = true;
        
        HidePanel(settingsPanel);
        ShowPanel(controlsPanel);
    }
    
    /// <summary>
    /// Closes controls and returns to settings menu.
    /// </summary>
    public void CloseControls()
    {
        isInControls = false;
        
        HidePanel(controlsPanel);
        ShowPanel(settingsPanel);
    }
    
    private void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }
    
    private void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
    
    #endregion
    
    #region Settings - Audio
    
    /// <summary>
    /// Sets the master volume. Called by volume slider.
    /// </summary>
    /// <param name="volume">Volume level (0-1)</param>
    public void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        AudioListener.volume = clampedVolume;
        
        Debug.Log($"[PauseManager] Volume set to: {clampedVolume:F2}");
    }
    
    #endregion
    
    #region Settings - Controls
    
    /// <summary>
    /// Sets mouse sensitivity and saves to PlayerPrefs. Called by sensitivity slider.
    /// </summary>
    /// <param name="sensitivity">Sensitivity value</param>
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, sensitivity);
        PlayerPrefs.Save();
        
        Debug.Log($"[PauseManager] Sensitivity set to: {sensitivity:F2}");
        
        // Notify player movement script if needed
        NotifySensitivityChange(sensitivity);
    }
    
    private void NotifySensitivityChange(float newSensitivity)
    {
        // Optional: Update PlayerMovement script directly
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            // If PlayerMovement has a public method to update sensitivity
            // player.UpdateLookSpeed(newSensitivity);
        }
    }
    
    #endregion
    
    #region Settings - Graphics
    
    /// <summary>
    /// Sets screen resolution. Called by resolution dropdown.
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
            
            Debug.Log($"[PauseManager] Resolution changed to {resolution.width}x{resolution.height}");
        }
        else
        {
            Debug.LogWarning($"[PauseManager] Invalid resolution index: {resolutionIndex}");
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
            
            case 2: // 1280x800
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
                Debug.LogWarning($"[PauseManager] Unknown resolution index: {index}");
                break;
        }
        
        return res;
    }
    
    #endregion
    
    #region Scene Management
    
    /// <summary>
    /// Returns to the main menu scene. Called by Main Menu button.
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log($"[PauseManager] Returning to main menu: {mainMenuSceneName}");
        
        UnfreezeGame();
        LoadMainMenu();
    }
    
    private void LoadMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("[PauseManager] Main menu scene name not set!");
        }
    }
    
    #endregion
    
    #region Public Query Methods
    
    /// <summary>
    /// Returns true if the game is currently paused.
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// Returns true if currently in any menu (pause, settings, or controls).
    /// </summary>
    public bool IsInMenu()
    {
        return isPaused || isInSettings || isInControls;
    }
    
    #endregion
}