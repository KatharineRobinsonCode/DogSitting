using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI References")]
    public GameObject pauseMenuCanvas;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject controlsPanel; // Add this - drag your ControlPanel here

    [Header("State")]
    private bool isPaused = false;
    private bool isInSettings = false;
    private bool isInControls = false; // Track if in controls submenu

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Make sure pause menu starts hidden
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    void Update()
    {
        // Press ESC to navigate back through menus
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInControls)
            {
                // If in controls, go back to settings
                CloseControls();
            }
            else if (isInSettings)
            {
                // If in settings, go back to pause menu
                CloseSettings();
            }
            else if (isPaused)
            {
                // If paused, resume game
                Resume();
            }
            else
            {
                // If playing, pause
                Pause();
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        
        // Show pause menu
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);
        
        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Freeze the game
        Time.timeScale = 0f;
        
        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Game Paused");
    }

    public void Resume()
    {
        isPaused = false;
        isInSettings = false;
        isInControls = false;
        
        // Hide all menus
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
            
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        // Unfreeze the game
        Time.timeScale = 1f;
        
        // Hide cursor (re-lock for FPS gameplay)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        Debug.Log("Game Resumed");
    }

    public void OpenSettings()
    {
        isInSettings = true;
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        isInSettings = false;
        isInControls = false; // Also reset controls state
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    // New method for opening controls
    public void OpenControls()
    {
        isInControls = true;
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    // New method for closing controls
    public void CloseControls()
    {
        isInControls = false;
        
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
        
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        // Unfreeze time before loading new scene
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        Debug.Log("Sensitivity set to: " + sensitivity);
    }

    public void SetResolution(int index)
    {
        switch (index)
        {
            case 0: Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow); break;
            case 1: Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow); break;
            case 2: Screen.SetResolution(1280, 800, FullScreenMode.FullScreenWindow); break;
            case 3: Screen.SetResolution(2560, 1440, FullScreenMode.FullScreenWindow); break;
            case 4: Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow); break;
        }
        
        Debug.Log("Resolution changed to option: " + index);
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}