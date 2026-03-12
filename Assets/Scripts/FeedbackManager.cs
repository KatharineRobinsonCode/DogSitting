using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages on-screen feedback messages for player actions.
/// Displays temporary color-coded messages for success, errors, and info.
/// </summary>
public class FeedbackManager : MonoBehaviour
{
    #region Message Types
    
    public enum MessageType
    {
        Success,
        Error,
        Info
    }
    
    #endregion
    
    #region Singleton
    
    public static FeedbackManager Instance { get; private set; }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("UI References")]
    [Tooltip("Text component for displaying feedback messages")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    
    [Header("Display Settings")]
    [Tooltip("How long messages stay on screen (seconds)")]
    [SerializeField] private float displayDuration = 2f;
    
    [Header("Message Colors")]
    [Tooltip("Color for success messages")]
    [SerializeField] private Color successColor = Color.green;
    
    [Tooltip("Color for error messages")]
    [SerializeField] private Color errorColor = Color.red;
    
    [Tooltip("Color for info messages")]
    [SerializeField] private Color infoColor = Color.white;
    
    #endregion
    
    #region Private Fields
    
    private Coroutine activeMessageCoroutine;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeUI();
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
    
    private void InitializeUI()
    {
        if (feedbackText == null)
        {
            Debug.LogError("[FeedbackManager] Feedback text not assigned!");
            return;
        }
        
        ClearMessage();
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Displays a temporary message to the player with the specified type and color.
    /// Automatically replaces any currently displayed message.
    /// </summary>
    /// <param name="message">Text to display</param>
    /// <param name="type">Message type (Success, Error, or Info)</param>
    public void ShowMessage(string message, MessageType type = MessageType.Info)
    {
        if (!ValidateMessage(message))
        {
            return;
        }
        
        StopCurrentMessage();
        DisplayNewMessage(message, type);
    }
    
    /// <summary>
    /// Immediately clears any displayed message.
    /// </summary>
    public void ClearMessage()
    {
        StopCurrentMessage();
        
        if (feedbackText != null)
        {
            feedbackText.text = string.Empty;
            feedbackText.gameObject.SetActive(false);
        }
    }
    
    #endregion
    
    #region Private Methods - Validation
    
    private bool ValidateMessage(string message)
    {
        if (feedbackText == null)
        {
            Debug.LogWarning("[FeedbackManager] Cannot show message: feedback text not assigned");
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("[FeedbackManager] Attempted to show empty message");
            return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Private Methods - Message Management
    
    private void StopCurrentMessage()
    {
        if (activeMessageCoroutine != null)
        {
            StopCoroutine(activeMessageCoroutine);
            activeMessageCoroutine = null;
        }
    }
    
    private void DisplayNewMessage(string message, MessageType type)
    {
        SetMessageColor(type);
        activeMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(message));
    }
    
    private void SetMessageColor(MessageType type)
    {
        feedbackText.color = GetColorForMessageType(type);
    }
    
    private Color GetColorForMessageType(MessageType type)
    {
        switch (type)
        {
            case MessageType.Success:
                return successColor;
            
            case MessageType.Error:
                return errorColor;
            
            case MessageType.Info:
                return infoColor;
            
            default:
                Debug.LogWarning($"[FeedbackManager] Unknown message type: {type}");
                return infoColor;
        }
    }
    
    #endregion
    
    #region Coroutines
    
    private IEnumerator DisplayMessageCoroutine(string message)
    {
        ShowMessageUI(message);
        
        yield return new WaitForSeconds(displayDuration);
        
        HideMessageUI();
        activeMessageCoroutine = null;
    }
    
    private void ShowMessageUI(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
    }
    
    private void HideMessageUI()
    {
        feedbackText.gameObject.SetActive(false);
        feedbackText.text = string.Empty;
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Changes the display duration for all future messages.
    /// </summary>
    public void SetDisplayDuration(float duration)
    {
        if (duration <= 0)
        {
            Debug.LogWarning($"[FeedbackManager] Invalid duration: {duration}. Must be > 0");
            return;
        }
        
        displayDuration = duration;
    }
    
    /// <summary>
    /// Checks if a message is currently being displayed.
    /// </summary>
    public bool IsMessageActive()
    {
        return activeMessageCoroutine != null;
    }
    
    /// <summary>
    /// Shows a success message (convenience method).
    /// </summary>
    public void ShowSuccess(string message)
    {
        ShowMessage(message, MessageType.Success);
    }
    
    /// <summary>
    /// Shows an error message (convenience method).
    /// </summary>
    public void ShowError(string message)
    {
        ShowMessage(message, MessageType.Error);
    }
    
    /// <summary>
    /// Shows an info message (convenience method).
    /// </summary>
    public void ShowInfo(string message)
    {
        ShowMessage(message, MessageType.Info);
    }
    
    #endregion
}