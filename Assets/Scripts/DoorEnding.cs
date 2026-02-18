using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Interactable door that triggers an ending sequence when the player leaves early.
/// Implements IInteractable to work with the player interaction system.
/// </summary>
public class DoorEnding : MonoBehaviour, IInteractable
{
    #region Serialized Fields
    
    [Header("Player Reference")]
    [Tooltip("Reference to player transform (for distance checks if needed)")]
    [SerializeField] private Transform player;
    
    [Header("Interaction Settings")]
    [Tooltip("Maximum distance for interaction prompt")]
    [SerializeField] private float interactDistance = 3f;
    
    [Header("Ending UI")]
    [Tooltip("Canvas containing the ending screen")]
    [SerializeField] private GameObject endingCanvas;
    
    [Tooltip("Text component for ending message")]
    [SerializeField] private TextMeshProUGUI endingText;
    
    [Header("Ending Content")]
    [TextArea(3, 10)]
    [Tooltip("Message displayed when ending is triggered")]
    [SerializeField] private string endingMessage = 
        "ENDING 1/5\n\n" +
        "You left your shift early.\n" +
        "You missed an important text on the drive home because your boss fired you...\n" +
        "Don't you wish you stayed so you could meet the dog?";
    
    #endregion
    
    #region Private Fields
    
    private bool hasTriggeredEnding = false;
    
    // Constants
    private const string ENDING_CANVAS_NAME = "EndingPanelCanvas";
    private const string ENDING_PANEL_NAME = "EndingPanel";
    
    #endregion
    
    #region IInteractable Implementation
    
    public string GetInteractionPrompt()
    {
        return "Press E to Leave Early";
    }
    
    public void Interact(PlayerInteraction player)
    {
        if (!hasTriggeredEnding)
        {
            TriggerEnding();
        }
    }
    
    #endregion
    
    #region Ending System
    
    private void TriggerEnding()
    {
        hasTriggeredEnding = true;
        
        Debug.Log("[DoorEnding] Ending sequence triggered");
        
        if (SetupEndingUI())
        {
            DisplayEndingScreen();
            PauseGameForEnding();
        }
        else
        {
            Debug.LogError("[DoorEnding] Failed to display ending screen");
        }
    }
    
    private bool SetupEndingUI()
    {
        if (!EnsureEndingCanvasExists())
        {
            return false;
        }
        
        EnsureEndingPanelActive();
        EnsureEndingTextExists();
        
        return true;
    }
    
    private bool EnsureEndingCanvasExists()
    {
        if (endingCanvas != null)
        {
            return true;
        }
        
        // Fallback: try to find by name
        endingCanvas = GameObject.Find(ENDING_CANVAS_NAME);
        
        if (endingCanvas != null)
        {
            Debug.LogWarning($"[DoorEnding] Found ending canvas by name. Consider assigning it in Inspector.");
            return true;
        }
        
        Debug.LogError($"[DoorEnding] Ending canvas not found! Looking for: {ENDING_CANVAS_NAME}");
        return false;
    }
    
    private void EnsureEndingPanelActive()
    {
        Transform panel = endingCanvas.transform.Find(ENDING_PANEL_NAME);
        
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[DoorEnding] Could not find child panel: {ENDING_PANEL_NAME}");
        }
    }
    
    private void EnsureEndingTextExists()
    {
        if (endingText != null)
        {
            return;
        }
        
        // Fallback: search in children
        endingText = endingCanvas.GetComponentInChildren<TextMeshProUGUI>();
        
        if (endingText != null)
        {
            Debug.LogWarning("[DoorEnding] Found ending text in children. Consider assigning it in Inspector.");
        }
        else
        {
            Debug.LogError("[DoorEnding] No TextMeshProUGUI component found for ending text!");
        }
    }
    
    private void DisplayEndingScreen()
    {
        if (endingCanvas != null)
        {
            endingCanvas.SetActive(true);
        }
        
        if (endingText != null)
        {
            endingText.text = endingMessage;
        }
    }
    
    private void PauseGameForEnding()
    {
        Time.timeScale = 0f;
        ShowCursor();
    }
    
    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    #endregion
    
    #region Button Callbacks
    
    /// <summary>
    /// Restarts the current scene. Called by Retry button.
    /// </summary>
    public void RetryGame()
    {
        Debug.Log("[DoorEnding] Restarting scene");
        
        ResumeTime();
        ReloadCurrentScene();
    }
    
    /// <summary>
    /// Quits the application. Called by Quit button.
    /// </summary>
    public void QuitToDesktop()
    {
        Debug.Log("[DoorEnding] Quitting game");
        
        #if UNITY_EDITOR
        QuitEditor();
        #else
        QuitApplication();
        #endif
    }
    
    #endregion
    
    #region Game Flow Helpers
    
    private void ResumeTime()
    {
        Time.timeScale = 1f;
    }
    
    private void ReloadCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
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
    
    #region Public Utility Methods
    
    /// <summary>
    /// Checks if the ending has been triggered.
    /// </summary>
    public bool HasEndingBeenTriggered()
    {
        return hasTriggeredEnding;
    }
    
    /// <summary>
    /// Manually trigger the ending (for testing or scripted events).
    /// </summary>
    public void ForceEndingTrigger()
    {
        if (!hasTriggeredEnding)
        {
            TriggerEnding();
        }
    }
    
    /// <summary>
    /// Reset the ending state (useful for testing).
    /// </summary>
    public void ResetEndingState()
    {
        hasTriggeredEnding = false;
        
        if (endingCanvas != null)
        {
            endingCanvas.SetActive(false);
        }
        
        ResumeTime();
    }
    
    #endregion
}