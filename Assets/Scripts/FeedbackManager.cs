using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;
    
    [Header("UI")]
    public TextMeshProUGUI feedbackText;
    
    [Header("Settings")]
    public float displayDuration = 2f;
    public Color successColor = Color.green;
    public Color errorColor = Color.red;
    public Color infoColor = Color.white;
    
    private Coroutine currentFeedback;
    
    void Awake()
    {
        // Singleton pattern - only one FeedbackManager exists
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
        if (feedbackText != null)
        {
            feedbackText.text = "";
            feedbackText.gameObject.SetActive(false);
        }
    }
    
    // Show a message to the player
    public void ShowMessage(string message, MessageType type = MessageType.Info)
    {
        if (feedbackText == null) return;
        
        // Stop any existing message
        if (currentFeedback != null)
        {
            StopCoroutine(currentFeedback);
        }
        
        // Set color based on message type
        switch (type)
        {
            case MessageType.Success:
                feedbackText.color = successColor;
                break;
            case MessageType.Error:
                feedbackText.color = errorColor;
                break;
            case MessageType.Info:
                feedbackText.color = infoColor;
                break;
        }
        
        // Show the message
        currentFeedback = StartCoroutine(DisplayMessage(message));
    }
    
    private IEnumerator DisplayMessage(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(displayDuration);
        
        feedbackText.gameObject.SetActive(false);
        feedbackText.text = "";
    }
    
    public enum MessageType
    {
        Success,
        Error,
        Info
    }
}