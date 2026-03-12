using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles ending triggers when player interacts with the exit door.
/// Checks if shift is complete before allowing player to leave.
/// </summary>
public class DoorEnding : MonoBehaviour, IInteractable
{
    #region Serialized Fields
    
    [Header("Scene Transitions")]
    [Tooltip("Scene to load when shift is complete (e.g., 'DrivingScene')")]
    [SerializeField] private string nextSceneName = "DrivingScene";
    
    [Header("Early Exit Ending")]
    [Tooltip("Canvas containing the ending UI")]
    [SerializeField] private GameObject endingCanvas;
    
    [Tooltip("Panel with ending text and buttons")]
    [SerializeField] private GameObject endingPanel;
    
    [Header("Task Requirement")]
    [Tooltip("Task text that allows leaving (e.g., 'Leave cafe')")]
    [SerializeField] private string allowedLeaveTask = "Leave cafe";
    
    [Header("Debug")]
    [Tooltip("Enable detailed logging")]
    [SerializeField] private bool enableDebugLogs = true;
    
    #endregion
    
    #region Constants
    
    private const string ENDING_CANVAS_NAME = "EndingPanelCanvas";
    private const string ENDING_PANEL_NAME = "EndingPanel";
    
    #endregion
    
    #region Private Fields
    
    private bool hasTriggeredEnding = false;
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Returns true if ending has been triggered.
    /// </summary>
    public bool HasEndingBeenTriggered => hasTriggeredEnding;
    
    #endregion
    
    #region IInteractable Implementation
    
    public string GetInteractionPrompt()
    {
        if (CanLeave())
        {
            return "Press E to leave cafe";
        }
        
        return "Press E to leave (shift incomplete)";
    }
    
    public void Interact(PlayerInteraction player)
    {
        LogDebug("[DoorEnding] Player interacted with door");
        
        if (CanLeave())
        {
            LeaveAndContinue();
        }
        else
        {
            TriggerEarlyExitEnding();
        }
    }
    
    #endregion
    
    #region Leave Logic
    
    private bool CanLeave()
    {
        // Check if TaskManager says it's okay to leave
        if (TaskManager.Instance == null)
        {
            LogDebug("[DoorEnding] TaskManager not found - allowing exit");
            return true; // Fallback: allow if no task manager
        }
        
        bool allowed = TaskManager.Instance.IsCurrentTask(allowedLeaveTask);
        LogDebug($"[DoorEnding] Can leave? {allowed} (Current task: '{TaskManager.Instance.CurrentTask}', Required: '{allowedLeaveTask}')");
        
        return allowed;
    }
    
    private void LeaveAndContinue()
    {
        LogDebug($"[DoorEnding] Shift complete! Loading next scene: {nextSceneName}");
        
        // Hide task UI
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.HideTask();
        }
        
        // Show transition feedback
        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.ShowSuccess("Shift complete! Going home...");
        }
        
        // Load next scene
        LoadNextScene();
    }
    
    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("[DoorEnding] Next scene name not set! Cannot transition.");
            return;
        }
        
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadSceneWithDelay(nextSceneName, 1f);
        }
        else
        {
            // Fallback: direct scene load
            SceneManager.LoadScene(nextSceneName);
        }
    }
    
    #endregion
    
    #region Early Exit Ending
    
    private void TriggerEarlyExitEnding()
    {
        if (hasTriggeredEnding)
        {
            return;
        }
        
        hasTriggeredEnding = true;
        
        LogDebug("[DoorEnding] Triggering early exit ending");
        
        SetupEndingUI();
        DisplayEndingScreen();
        PauseGameForEnding();
    }
    
    private void SetupEndingUI()
    {
        EnsureEndingCanvasExists();
        EnsureEndingPanelActive();
    }
    
    private void EnsureEndingCanvasExists()
    {
        if (endingCanvas == null)
        {
            endingCanvas = GameObject.Find(ENDING_CANVAS_NAME);
            
            if (endingCanvas == null)
            {
                Debug.LogWarning($"[DoorEnding] Ending canvas '{ENDING_CANVAS_NAME}' not found");
            }
        }
    }
    
    private void EnsureEndingPanelActive()
    {
        if (endingPanel == null && endingCanvas != null)
        {
            Transform panelTransform = endingCanvas.transform.Find(ENDING_PANEL_NAME);
            
            if (panelTransform != null)
            {
                endingPanel = panelTransform.gameObject;
            }
            else
            {
                Debug.LogWarning($"[DoorEnding] Ending panel '{ENDING_PANEL_NAME}' not found");
            }
        }
    }
    
    private void DisplayEndingScreen()
    {
        if (endingCanvas != null)
        {
            endingCanvas.SetActive(true);
        }
        
        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }
    }
    
    private void PauseGameForEnding()
    {
        Time.timeScale = 0f;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    #endregion
    
    #region Button Callbacks
    
    /// <summary>
    /// Called by Retry button - reloads the current scene.
    /// </summary>
    public void RetryGame()
    {
        LogDebug("[DoorEnding] Retrying game");
        
        ResumeTime();
        ReloadCurrentScene();
    }
    
    /// <summary>
    /// Called by Quit button - exits the application.
    /// </summary>
    public void QuitToDesktop()
    {
        LogDebug("[DoorEnding] Quitting to desktop");
        
        ResumeTime();
        
        #if UNITY_EDITOR
        QuitEditor();
        #else
        QuitApplication();
        #endif
    }
    
    private void ResumeTime()
    {
        Time.timeScale = 1f;
    }
    
    private void ReloadCurrentScene()
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.ReloadScene();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
    /// Manually trigger the ending (for testing or other scripts).
    /// </summary>
    public void ForceEndingTrigger()
    {
        TriggerEarlyExitEnding();
    }
    
    /// <summary>
    /// Reset ending state (useful for testing).
    /// </summary>
    public void ResetEndingState()
    {
        hasTriggeredEnding = false;
        
        if (endingCanvas != null)
        {
            endingCanvas.SetActive(false);
        }
        
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }
        
        ResumeTime();
    }
    
    #endregion
    
    #region Debug Helpers
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    #endregion
}