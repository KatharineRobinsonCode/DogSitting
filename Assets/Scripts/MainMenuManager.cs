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
    [Tooltip("Input field where the player types their name before starting")]
    [SerializeField] private TMP_InputField nameInputField;
    
    [Tooltip("Yarn Spinner's variable storage — used to set $PlayerName so dialogue can use it")]
    [SerializeField] private VariableStorageBehaviour variableStorage;
    
    [Header("Audio")]
    [Tooltip("Background music that plays on the main menu")]
    [SerializeField] private AudioSource menuMusic;
    
    [Header("Scene Settings")]
    [Tooltip("The name of the first gameplay scene to load when starting a new game")]
    [SerializeField] private string gameSceneName = "Pub";
    
    [Header("Default Settings")]
    [Tooltip("Default mouse sensitivity if the player hasn't changed it yet")]
    [SerializeField] private float defaultSensitivity = 2f;
    
    [Tooltip("Default audio volume (0 = silent, 1 = full volume)")]
    [SerializeField] private float defaultVolume = 1f;
    
    #endregion
    
    #region Private Fields
    
    // The Yarn variable name for the player's name — must match exactly what's in the .yarn files
    private const string YARN_PLAYER_NAME_VARIABLE = "$PlayerName";
    
    // The PlayerPrefs key used to save which scene the player last played
    private const string LAST_SCENE_PREF_KEY = "LastScene";
    
    // The PlayerPrefs key used to save mouse sensitivity
    private const string SENSITIVITY_PREF_KEY = "MouseSensitivity";
    
    // The name of this scene — used to make sure we never accidentally save the main menu as a continue point
    private const string MAIN_MENU_SCENE_NAME = "MainMenu";
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        // Run setup as soon as the menu scene loads
        InitializeMenu();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeMenu()
    {
        SetupCursor();    // Make cursor visible so player can click buttons
        StartMenuMusic(); // Play background music
        LoadSettings();   // Apply any previously saved settings
    }
    
    private void SetupCursor()
    {
        // Unlike gameplay scenes, the cursor needs to be free and visible on the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    private void StartMenuMusic()
    {
        // Only play if an audio source is assigned and it's not already playing
        if (menuMusic != null && !menuMusic.isPlaying)
        {
            menuMusic.Play();
        }
    }
    
    private void LoadSettings()
    {
        // Read saved settings from PlayerPrefs, falling back to defaults if none exist
        // PlayerPrefs is Unity's simple key-value store that persists between sessions
        float savedSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_PREF_KEY, defaultSensitivity);
        AudioListener.volume = PlayerPrefs.GetFloat("Volume", defaultVolume);
    }
    
    #endregion
    
    #region Game Flow - Start/Continue
    
    /// <summary>
    /// Starts a fresh new game. Clears any existing save data first so the 
    /// player begins with an empty inventory and no previous scene saved.
    /// Called by the New Game button.
    /// </summary>
    public void StartNewGame()
    {
        // Don't start if the player hasn't entered a name yet
        if (!ValidatePlayerName()) { ShowNameRequiredWarning(); return; }
        
        // Wipe all previous save data so the new game starts completely fresh
        PlayerPrefs.DeleteKey("InventoryItems");       // Clear any previously held items
        PlayerPrefs.DeleteKey("PlayerName");           // Clear old name
        PlayerPrefs.DeleteKey(LAST_SCENE_PREF_KEY);   // Clear last scene so Continue won't find stale data
        PlayerPrefs.Save();                            // Flush changes to disk immediately
        
        SavePlayerName();  // Save the new name the player just entered
        LoadGameScene();   // Load the first gameplay scene
    }
    
    /// <summary>
    /// Continues from where the player last left off.
    /// If no save exists, falls back to starting a new game.
    /// Called by the Continue button.
    /// </summary>
    public void ContinueGame()
    {
        if (HasSavedGame())
        {
            LoadSavedGame(); // Resume from saved scene
        }
        else
        {
            // No save found — treat it like a new game instead
            Debug.Log("[MainMenuManager] No save found, starting new game");
            StartNewGame();
        }
    }
    
    /// <summary>
    /// Exits the application. In the Unity Editor this stops play mode instead.
    /// Called by the Exit button.
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("[MainMenuManager] Exiting game");
        
        // Different exit behaviour in editor vs built game
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
        // Check the input field exists in the scene
        if (nameInputField == null)
        {
            Debug.LogError("[MainMenuManager] Name input field not assigned!");
            return false;
        }
        
        // Check the player actually typed something (not blank or just spaces)
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            return false;
        }
        
        return true;
    }
    
    private void ShowNameRequiredWarning()
    {
        // Log a warning — could also show a UI message to the player here
        Debug.LogWarning("[MainMenuManager] Player must enter a name first!");
    }
    
    private void SavePlayerName()
    {
        // Trim removes any accidental leading/trailing spaces the player might have typed
        string playerName = nameInputField.text.Trim();
        
        // Save to PlayerPrefs so other scenes can restore it after loading
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save(); // Flush to disk
        
        // Also set it in Yarn's variable storage so $PlayerName works in dialogue immediately
        if (variableStorage != null)
        {
            try
            {
                variableStorage.SetValue(YARN_PLAYER_NAME_VARIABLE, playerName);
                Debug.Log($"[MainMenuManager] Saved player name: {playerName}");
            }
            catch (System.Exception e)
            {
                // Catch any Yarn errors without crashing the game
                Debug.LogError($"[MainMenuManager] Failed to save player name: {e.Message}");
            }
        }
    }
    
    #endregion
    
    #region Save System
    
    private bool HasSavedGame()
    {
        // Simply checks whether a last scene has ever been saved to PlayerPrefs
        return PlayerPrefs.HasKey(LAST_SCENE_PREF_KEY);
    }
    
    private void LoadSavedGame()
    {
        // Read which scene to load from PlayerPrefs
        string savedScene = PlayerPrefs.GetString(LAST_SCENE_PREF_KEY);
        
        // Safety check — if the key exists but is empty, start fresh instead
        if (string.IsNullOrEmpty(savedScene)) { StartNewGame(); return; }
        
        // Debug — confirms what name PlayerPrefs has stored
        Debug.Log($"[YarnVariableRestorer] PlayerPrefs PlayerName: '{PlayerPrefs.GetString("PlayerName", "NOT FOUND")}'");
        
        // Restore the player's name into Yarn so dialogue uses it correctly in the loaded scene
        string savedName = PlayerPrefs.GetString("PlayerName", "");
        if (!string.IsNullOrEmpty(savedName) && variableStorage != null)
            variableStorage.SetValue(YARN_PLAYER_NAME_VARIABLE, savedName);

        Debug.Log($"[MainMenuManager] Loading saved game: {savedScene}");
        
        // Load the scene the player was last in
        SceneManager.LoadScene(savedScene);
    }
    
    /// <summary>
    /// Saves the current scene name to PlayerPrefs so Continue knows where to return to.
    /// Called automatically by SceneSaveHelper when each gameplay scene loads.
    /// Static so it can be called without needing a reference to this manager.
    /// </summary>
    public static void SaveCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        
        // Never save the main menu itself as a continue point — that would break Continue
        if (currentScene == MAIN_MENU_SCENE_NAME)
        {
            return;
        }
        
        // Write the scene name and flush to disk
        PlayerPrefs.SetString(LAST_SCENE_PREF_KEY, currentScene);
        PlayerPrefs.Save();
        
        Debug.Log($"[MainMenuManager] Saved current scene: {currentScene}");
    }
    
    /// <summary>
    /// Wipes all saved game data. Useful for a "Delete Save" feature.
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
        // Safety check — make sure a scene name has actually been set in the Inspector
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenuManager] Game scene name not set!");
            return;
        }
        
        Debug.Log($"[MainMenuManager] Loading game scene: {gameSceneName}");
        SceneManager.LoadScene(gameSceneName); // Triggers Unity to load the named scene
    }
    
    #endregion
    
    #region Settings - Audio
    
    /// <summary>
    /// Sets the master volume for the whole game.
    /// Called by the volume slider in settings.
    /// </summary>
    /// <param name="volume">Volume level between 0 (silent) and 1 (full)</param>
    public void SetVolume(float volume)
    {
        // Clamp ensures the value can never go below 0 or above 1
        float clampedVolume = Mathf.Clamp01(volume);
        
        // AudioListener.volume controls all audio in the game globally
        AudioListener.volume = clampedVolume;
        
        // Save so it's restored next session
        PlayerPrefs.SetFloat("Volume", clampedVolume);
        PlayerPrefs.Save();
        
        Debug.Log($"[MainMenuManager] Volume set to: {clampedVolume:F2}");
    }
    
    #endregion
    
    #region Settings - Controls
    
    /// <summary>
    /// Sets mouse sensitivity and saves it to PlayerPrefs.
    /// Called by the sensitivity slider in settings.
    /// Each gameplay scene reads this value on Start and applies it to MouseLook.
    /// </summary>
    /// <param name="sensitivity">Sensitivity value — should match slider range (e.g. 10-200)</param>
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SENSITIVITY_PREF_KEY, sensitivity);
        PlayerPrefs.Save(); // Flush to disk so it persists between sessions
        
        Debug.Log($"[MainMenuManager] Sensitivity set to: {sensitivity:F2}");
    }
    
    #endregion
    
    #region Settings - Graphics
    
    /// <summary>
    /// Changes the screen resolution based on a dropdown selection.
    /// Called by the resolution dropdown in settings.
    /// </summary>
    /// <param name="resolutionIndex">Index matching a preset resolution (0=1080p, 1=720p, etc.)</param>
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = GetResolutionFromIndex(resolutionIndex);
        
        if (resolution.width > 0)
        {
            // Apply the resolution — FullScreenWindow keeps it borderless fullscreen
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
        // Maps dropdown index numbers to actual pixel dimensions
        Resolution res = new Resolution();
        
        switch (index)
        {
            case 0: // Standard 1080p — most common monitor resolution
                res.width = 1920;
                res.height = 1080;
                break;
            
            case 1: // 720p — lower quality, better performance
                res.width = 1280;
                res.height = 720;
                break;
            
            case 2: // Steam Deck native resolution
                res.width = 1280;
                res.height = 800;
                break;
            
            case 3: // 1440p — high quality
                res.width = 2560;
                res.height = 1440;
                break;
            
            case 4: // 4K — highest quality
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
        // Closes the built game executable
        Application.Quit();
    }
    
    #if UNITY_EDITOR
    private void QuitEditor()
    {
        // Stops play mode in the Unity Editor instead of closing the application
        UnityEditor.EditorApplication.isPlaying = false;
    }
    #endif
    
    #endregion
    
    #region Public Query Methods
    
    /// <summary>
    /// Public wrapper so UI buttons or other scripts can check if a save exists.
    /// Useful for greying out the Continue button if there's nothing to continue.
    /// </summary>
    public bool DoesSaveExist()
    {
        return HasSavedGame();
    }
    
    /// <summary>
    /// Returns the name of the saved scene, or empty string if no save exists.
    /// Useful for showing the player where they'll continue from.
    /// </summary>
    public string GetSavedSceneName()
    {
        return HasSavedGame() ? PlayerPrefs.GetString(LAST_SCENE_PREF_KEY) : string.Empty;
    }
    
    #endregion
}

/// <summary>
/// Small helper component that automatically records the current scene 
/// to PlayerPrefs the moment it loads. Attach to a GameObject in every 
/// gameplay scene (Pub, Driving, House) so Continue always knows the 
/// most recent scene the player reached.
/// </summary>
public class SceneSaveHelper : MonoBehaviour
{
    #region Unity Lifecycle
    
    private void Start()
    {
        // Save as soon as this scene loads — before the player does anything
        SaveCurrentScene();
    }
    
    #endregion
    
    #region Save Logic
    
    private void SaveCurrentScene()
    {
        // Delegate to MainMenuManager's static method which handles the actual save
        // Static means we don't need a MainMenuManager instance in this scene
        MainMenuManager.SaveCurrentScene();
    }
    
    #endregion
}