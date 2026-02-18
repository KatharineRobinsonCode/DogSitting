using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages phone UI interactions including AirDrop notifications and text messaging.
/// Handles both horror elements (creepy photos) and narrative choices (dog sitting request).
/// </summary>
public class PhoneManager : MonoBehaviour
{
    #region Singleton
    
    public static PhoneManager Instance { get; private set; }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("Phone UI Components")]
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private CanvasGroup phoneCanvasGroup;
    [SerializeField] private Image airdropImage;
    [SerializeField] private GameObject actionButtons;
    
    [Header("Player Dialogue")]
    [SerializeField] private TextMeshProUGUI playerDialogueText;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip notificationSound;
    [SerializeField] private AudioClip scaryZoomSound;
    [SerializeField] private AudioClip textMessageSound;
    
    [Header("Game Ending")]
    [SerializeField] private GameObject endingCanvas;
    [SerializeField] private TextMeshProUGUI endingText;
    
    [TextArea(3, 10)]
    [SerializeField] private string declineEndingMessage =
        "ENDING 2/5\n\nYou ignored your best friend.\n" +
        "You never met the dog...\n" +
        "But something else found them...";
    
    [Header("Text Message UI")]
    [SerializeField] private GameObject textMessagePanel;
    [SerializeField] private TextMeshProUGUI contactNameText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject messageAcceptButton;
    [SerializeField] private GameObject messageDeclineButton;
    
    [Header("Text Message Content")]
    [SerializeField] private string contactName = "Bestie 🐾";
    
    [TextArea(3, 10)]
    [SerializeField] private string dogSitMessage =
        "Hey!! Are you free after your shift? \n" +
        "I need someone to watch my dog tonight... \n" +
        "Please please please 🐶🙏";
    
    [Header("Timing Settings")]
    [Tooltip("Delay before showing phone UI")]
    [SerializeField] private float phoneShowDelay = 0.5f;
    
    [Tooltip("Delay before showing message buttons")]
    [SerializeField] private float buttonShowDelay = 0.5f;
    
    [Tooltip("Simulated typing delay before player's reply")]
    [SerializeField] private float playerTypingDelay = 0.8f;
    
    [Tooltip("Simulated typing delay before friend's response")]
    [SerializeField] private float friendTypingDelay = 1.2f;
    
    [Tooltip("Time to read final message before closing")]
    [SerializeField] private float messageReadTime = 2f;
    
    [Tooltip("Duration of player reaction to creepy photo")]
    [SerializeField] private float creepyReactionTime = 3f;
    
    [Header("Animation Settings")]
    [Tooltip("Duration of photo zoom animation")]
    [SerializeField] private float zoomDuration = 0.8f;
    
    [Tooltip("Scale multiplier for zoomed photo")]
    [SerializeField] private float zoomScale = 5.0f;
    
    #endregion
    
    #region Private Fields
    
    private Vector3 originalImageScale;
    private Vector3 originalImagePosition;
    
    private Action onAcceptedCallback;
    private Action onDeclinedCallback;
    
    // Constants
    private const float PHONE_VISIBLE_ALPHA = 1f;
    private const float PHONE_HIDDEN_ALPHA = 0f;
    private const float SCROLL_TOP = 1f;
    private const float SCROLL_BOTTOM = 0f;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
        CacheOriginalImageTransform();
        InitializePhoneState();
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
    
    private void CacheOriginalImageTransform()
    {
        if (airdropImage != null)
        {
            originalImageScale = airdropImage.transform.localScale;
            originalImagePosition = airdropImage.transform.localPosition;
        }
    }
    
    private void InitializePhoneState()
    {
        if (phonePanel != null)
        {
            phonePanel.SetActive(true);
        }
        
        HideAllPhoneChildren();
        HidePhoneCanvas();
    }
    
    private void HideAllPhoneChildren()
    {
        if (actionButtons != null)
        {
            actionButtons.SetActive(false);
        }
        
        if (playerDialogueText != null)
        {
            playerDialogueText.text = string.Empty;
        }
        
        if (textMessagePanel != null)
        {
            textMessagePanel.SetActive(false);
        }
        
        if (airdropImage != null)
        {
            airdropImage.gameObject.SetActive(false);
        }
    }
    
    private void HidePhoneCanvas()
    {
        if (phoneCanvasGroup != null)
        {
            phoneCanvasGroup.alpha = PHONE_HIDDEN_ALPHA;
        }
    }
    
    #endregion
    
    #region Public API - AirDrop System
    
    /// <summary>
    /// Displays a creepy AirDrop notification with an image.
    /// </summary>
    /// <param name="content">AirDrop message text (currently unused)</param>
    /// <param name="horrorPhoto">The creepy image to display</param>
    public void ReceiveAirdrop(string content = null, Sprite horrorPhoto = null)
    {
        StopAllCoroutines();
        
        PrepareForAirdrop();
        StartCoroutine(ProcessAirdrop(horrorPhoto));
    }
    
    #endregion
    
    #region Public API - Text Message System
    
    /// <summary>
    /// Displays a text message conversation with accept/decline options.
    /// </summary>
    /// <param name="onAccept">Callback invoked when player accepts</param>
    /// <param name="onDecline">Callback invoked when player declines</param>
    public void ReceiveTextMessage(Action onAccept = null, Action onDecline = null)
    {
        onAcceptedCallback = onAccept;
        onDeclinedCallback = onDecline;
        
        StopAllCoroutines();
        
        PrepareForTextMessage();
        StartCoroutine(ShowTextMessage());
    }
    
    #endregion
    
    #region Public API - Button Callbacks
    
    public void OnAcceptPressed()
    {
        StartCoroutine(EnlargeImageAndShowReaction());
    }
    
    public void OnDeclinePressed()
    {
        ClosePhone();
    }
    
    public void OnTextMessageAccepted()
    {
        StartCoroutine(HandleTextMessageAccepted());
    }
    
    public void OnTextMessageDeclined()
    {
        StartCoroutine(HandleTextMessageDeclined());
    }
    
    #endregion
    
    #region AirDrop - Private Methods
    
    private void PrepareForAirdrop()
    {
        HideTextMessageUI();
        
        if (actionButtons != null)
        {
            actionButtons.SetActive(false);
        }
        
        ResetAirdropImage();
    }
    
    private IEnumerator ProcessAirdrop(Sprite horrorPhoto)
    {
        ShowPhoneCanvas();
        
        yield return new WaitForEndOfFrame();
        
        DisplayAirdropImage(horrorPhoto);
        PlayNotificationSound();
        
        if (actionButtons != null)
        {
            actionButtons.SetActive(true);
        }
    }
    
    private void DisplayAirdropImage(Sprite photo)
    {
        if (airdropImage == null) return;
        
        if (photo != null)
        {
            airdropImage.sprite = photo;
            airdropImage.gameObject.SetActive(true);
        }
        else
        {
            airdropImage.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator EnlargeImageAndShowReaction()
    {
        if (actionButtons != null)
        {
            actionButtons.SetActive(false);
        }
        
        Image phoneBackground = GetPhoneBackground();
        if (phoneBackground != null)
        {
            phoneBackground.enabled = false;
        }
        
        yield return StartCoroutine(AnimateImageZoom());
        
        PlayScarySound();
        ShowPlayerReaction();
        
        yield return new WaitForSeconds(creepyReactionTime);
        
        if (phoneBackground != null)
        {
            phoneBackground.enabled = true;
        }
        
        ClosePhone();
    }
    
    private Image GetPhoneBackground()
    {
        return phonePanel != null ? phonePanel.GetComponent<Image>() : null;
    }
    
    private IEnumerator AnimateImageZoom()
    {
        Vector3 targetScale = originalImageScale * zoomScale;
        Vector3 targetPosition = Vector3.zero;
        
        float elapsed = 0f;
        
        while (elapsed < zoomDuration)
        {
            float progress = elapsed / zoomDuration;
            
            airdropImage.transform.localScale = Vector3.Lerp(
                originalImageScale, 
                targetScale, 
                progress
            );
            
            airdropImage.transform.localPosition = Vector3.Lerp(
                originalImagePosition, 
                targetPosition, 
                progress
            );
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure final values are set
        airdropImage.transform.localScale = targetScale;
        airdropImage.transform.localPosition = targetPosition;
    }
    
    private void PlayScarySound()
    {
        if (audioSource != null && scaryZoomSound != null)
        {
            audioSource.PlayOneShot(scaryZoomSound);
        }
    }
    
    private void ShowPlayerReaction()
    {
        if (playerDialogueText != null)
        {
            playerDialogueText.text = "Well that's creepy...";
            playerDialogueText.gameObject.SetActive(true);
        }
    }
    
    #endregion
    
    #region Text Message - Private Methods
    
    private void PrepareForTextMessage()
    {
        HideAirdropUI();
        ShowTextMessageUI();
        ClearMessageContent();
        HideMessageButtons();
    }
    
    private void HideAirdropUI()
    {
        ResetAirdropImage();
        
        if (actionButtons != null)
        {
            actionButtons.SetActive(false);
        }
        
        if (playerDialogueText != null)
        {
            playerDialogueText.text = string.Empty;
            playerDialogueText.gameObject.SetActive(false);
        }
    }
    
    private void ResetAirdropImage()
    {
        if (airdropImage == null) return;
        
        airdropImage.gameObject.SetActive(false);
        airdropImage.transform.localScale = originalImageScale;
        airdropImage.transform.localPosition = originalImagePosition;
    }
    
    private void ShowTextMessageUI()
    {
        if (textMessagePanel != null)
        {
            textMessagePanel.SetActive(true);
        }
    }
    
    private void HideTextMessageUI()
    {
        if (textMessagePanel != null)
        {
            textMessagePanel.SetActive(false);
        }
    }
    
    private void ClearMessageContent()
    {
        if (contactNameText != null)
        {
            contactNameText.text = contactName;
        }
        
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }
    }
    
    private void HideMessageButtons()
    {
        if (messageAcceptButton != null)
        {
            messageAcceptButton.SetActive(false);
        }
        
        if (messageDeclineButton != null)
        {
            messageDeclineButton.SetActive(false);
        }
    }
    
    private IEnumerator ShowTextMessage()
    {
        PlayTextMessageSound();
        
        yield return new WaitForSeconds(phoneShowDelay);
        
        ShowPhoneCanvas();
        PlayNotificationSound();
        
        ResetScrollPosition();
        DisplayInitialMessage();
        
        yield return new WaitForSeconds(buttonShowDelay);
        
        ShowMessageButtons();
    }
    
    private void PlayTextMessageSound()
    {
        if (audioSource != null && textMessageSound != null)
        {
            audioSource.PlayOneShot(textMessageSound);
        }
    }
    
    private void PlayNotificationSound()
    {
        if (audioSource != null && notificationSound != null)
        {
            audioSource.PlayOneShot(notificationSound);
        }
    }
    
    private void ShowPhoneCanvas()
    {
        if (phoneCanvasGroup != null)
        {
            phoneCanvasGroup.alpha = PHONE_VISIBLE_ALPHA;
        }
    }
    
    private void ResetScrollPosition()
    {
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }
        
        ScrollRect scrollRect = GetMessageScrollRect();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = SCROLL_TOP;
        }
    }
    
    private void DisplayInitialMessage()
    {
        if (messageText != null)
        {
            messageText.text = dogSitMessage;
        }
    }
    
    private void ShowMessageButtons()
    {
        if (messageAcceptButton != null)
        {
            messageAcceptButton.SetActive(true);
        }
        
        if (messageDeclineButton != null)
        {
            messageDeclineButton.SetActive(true);
        }
    }
    
    private void AppendMessage(string message, bool addNewlineBefore = true)
    {
        if (messageText == null) return;
        
        if (addNewlineBefore)
        {
            messageText.text += "\n\n";
        }
        
        messageText.text += message;
        
        ScrollToBottomIfNeeded();
    }
    
    private void ScrollToBottomIfNeeded()
    {
        Canvas.ForceUpdateCanvases();
        
        ScrollRect scrollRect = GetMessageScrollRect();
        if (scrollRect == null) return;
        
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        
        if (content != null && viewport != null)
        {
            if (content.rect.height > viewport.rect.height)
            {
                scrollRect.verticalNormalizedPosition = SCROLL_BOTTOM;
            }
        }
    }
    
    private ScrollRect GetMessageScrollRect()
    {
        return textMessagePanel != null ? 
               textMessagePanel.GetComponentInChildren<ScrollRect>() : 
               null;
    }
    
    #endregion
    
    #region Text Message - Conversation Flows
    
    private IEnumerator HandleTextMessageAccepted()
    {
        HideMessageButtons();
        
        yield return new WaitForSeconds(playerTypingDelay);
        AppendMessage("Sure, I'll be there after my shift");
        
        yield return new WaitForSeconds(friendTypingDelay);
        AppendMessage("Bestie: YAYY thank you!! You're the best!!");
        
        yield return new WaitForSeconds(messageReadTime);
        
        CloseTextMessage();
        onAcceptedCallback?.Invoke();
    }
    
    private IEnumerator HandleTextMessageDeclined()
    {
        HideMessageButtons();
        
        yield return new WaitForSeconds(playerTypingDelay);
        AppendMessage("You: Sorry, can't tonight!");
        
        yield return new WaitForSeconds(friendTypingDelay);
        AppendMessage("Bestie: Aw no worries");
        
        yield return new WaitForSeconds(messageReadTime);
        
        ShowDeclineEnding();
        onDeclinedCallback?.Invoke();
    }
    
    #endregion
    
    #region Phone State Management
    
    public void ClosePhone()
    {
        if (playerDialogueText != null)
        {
            playerDialogueText.text = string.Empty;
        }
        
        ResetAirdropImage();
        HidePhoneCanvas();
        HideAllPhoneChildren();
    }
    
    private void CloseTextMessage()
    {
        HideTextMessageUI();
        
        if (airdropImage != null)
        {
            airdropImage.gameObject.SetActive(true);
        }
        
        ClosePhone();
    }
    
    #endregion
    
    #region Game Ending
    
    public void ShowDeclineEnding()
    {
        ClosePhone();
        
        if (endingCanvas != null)
        {
            endingCanvas.SetActive(true);
            
            Transform panel = endingCanvas.transform.Find("EndingPanel");
            if (panel != null)
            {
                panel.gameObject.SetActive(true);
            }
            
            if (endingText != null)
            {
                endingText.text = declineEndingMessage;
            }
        }
        
        PauseGame();
        ShowCursor();
    }
    
    private void PauseGame()
    {
        Time.timeScale = 0f;
    }
    
    private void ShowCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    #endregion
}