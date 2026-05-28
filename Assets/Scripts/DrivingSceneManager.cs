using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages the driving scene flow including intro, driving, and transitions.
/// </summary>
public class DrivingSceneManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Scene Flow")]
    [Tooltip("Intro sequence component")]
    [SerializeField] private IntroSequence introSequence;
    
    [Tooltip("Car controller component")]
    [SerializeField] private CarController carController;
    
    [Tooltip("Next scene to load when destination is reached")]
    [SerializeField] private string nextSceneName = "HouseScene";
    
    [Header("Crash Ending")]
    [Tooltip("Canvas for crash ending UI")]
    [SerializeField] private GameObject crashEndingCanvas;
    
    [Tooltip("Panel with crash ending text")]
    [SerializeField] private GameObject crashEndingPanel;
    
    [Header("Fade Settings")]
    [Tooltip("Image used for fade to black")]
    [SerializeField] private Image fadeImage;
    
    [Tooltip("Duration of fade to black")]
    [SerializeField] private float fadeDuration = 2f;
    
    [Header("Timing")]
    [Tooltip("Delay after intro before enabling car controls")]
    [SerializeField] private float delayAfterIntro = 0.5f;
    
    [Tooltip("Delay after reaching destination before fading")]
    [SerializeField] private float delayBeforeFade = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    #endregion
    
    #region Private Fields
    
    private bool hasReachedDestination = false;
    private bool hasCrashed = false;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeScene();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeScene()
    {
        LogDebug("[DrivingScene] Initializing scene");
        
        // Disable car controls initially
        if (carController != null)
        {
            carController.DisableControls();
        }
        
        // Hide crash ending
        if (crashEndingCanvas != null)
        {
            crashEndingCanvas.SetActive(false);
        }
        
        // Setup fade image
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        }
        
        // Start intro sequence
        StartIntroSequence();
    }
    
    #endregion
    
    #region Intro Sequence
    
    private void StartIntroSequence()
    {
        if (introSequence != null)
        {
            LogDebug("[DrivingScene] Starting intro sequence");
            StartCoroutine(WaitForIntroToComplete());
        }
        else
        {
            Debug.LogWarning("[DrivingScene] IntroSequence not assigned, starting driving immediately");
            StartDriving();
        }
    }
    
    private IEnumerator WaitForIntroToComplete()
    {
        // Wait for intro to finish
        while (introSequence != null && !introSequence.IsComplete())
        {
            yield return null;
        }
        
        LogDebug("[DrivingScene] Intro complete");
        
        yield return new WaitForSeconds(delayAfterIntro);
        
        StartDriving();
    }
    
    #endregion
    
    #region Driving Phase
    
   private void StartDriving()
{
    LogDebug("[DrivingScene] Starting driving phase");
    
    if (TaskManager.Instance != null)
        TaskManager.Instance.ShowTask("Drive to the house");
    
    if (carController != null)
        carController.EnableControls();

    // Lock cursor for driving
    if (PauseManager.Instance != null)
        PauseManager.Instance.HideCursorPublic();

    FollowCar followCar = FindFirstObjectByType<FollowCar>();
    followCar?.StartFollowing();
}
    
    #endregion
    
    #region Crash Ending
    
    /// <summary>
    /// Called by RoadBoundary when car goes off road.
    /// </summary>
    public void TriggerCrashEnding()
    {
        if (hasCrashed || hasReachedDestination) return;
        
        hasCrashed = true;
        
        LogDebug("[DrivingScene] Crash ending triggered");
        
        // Stop the car
        if (carController != null)
        {
            carController.StopCar();
        }
        
        // Hide task
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.HideTask();
        }
        
        // Show crash ending
        DisplayCrashEnding();
    }
    
    private void DisplayCrashEnding()
    {
        if (crashEndingCanvas != null)
        {
            crashEndingCanvas.SetActive(true);
        }
        
        if (crashEndingPanel != null)
        {
            crashEndingPanel.SetActive(true);
        }
        
        // Pause game
        Time.timeScale = 0f;
        
        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    #endregion
    
    #region Destination Reached
    
    /// <summary>
    /// Called by DestinationTrigger when car reaches the house.
    /// </summary>
    public void ReachDestination()
    {
        if (hasReachedDestination || hasCrashed) return;
        
        hasReachedDestination = true;
        
        LogDebug("[DrivingScene] Destination reached");
        
        StartCoroutine(HandleArrivalSequence());
    }
    
    private IEnumerator HandleArrivalSequence()
    {
        // Stop the car
        if (carController != null)
        {
            carController.StopCar();
        }
        
        // Hide task
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.HideTask();
        }
        
        // Show success feedback
        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.ShowSuccess("You've arrived safely");
        }
        
        // Wait before fading
        yield return new WaitForSeconds(delayBeforeFade);
        
        // Fade to black
        yield return StartCoroutine(FadeToBlack());
        
        // Load next scene
        LoadNextScene();
    }
    
    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
        {
            yield break;
        }
        
        LogDebug("[DrivingScene] Fading to black");
        
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color endColor = new Color(0, 0, 0, 1);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        fadeImage.color = endColor;
    }
    
    private void LoadNextScene()
    {
        LogDebug($"[DrivingScene] Loading next scene: {nextSceneName}");
        
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(nextSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
    
    #endregion
    
    #region Button Callbacks
    
    /// <summary>
    /// Called by Retry button in crash ending.
    /// </summary>
    public void RetryDriving()
    {
        LogDebug("[DrivingScene] Retrying driving scene");
        
        Time.timeScale = 1f;
        
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.ReloadScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    
    /// <summary>
    /// Called by Quit button in crash ending.
    /// </summary>
    public void QuitToMenu()
    {
        LogDebug("[DrivingScene] Quitting to main menu");
        
        Time.timeScale = 1f;
        
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene("MainMenu");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
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