using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{
    public static PhoneManager Instance;
   
    [Header("UI Components")]
    public GameObject phonePanel;        
    public CanvasGroup phoneCanvasGroup; 
    public Image airdropImage;    
    public GameObject actionButtons; 

    [Header("Dialogue System")]
    public TextMeshProUGUI playerDialogueText; 

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip notificationSound; 
    public AudioClip scaryZoomSound;
    public AudioClip textMessageSound;

    [Header("Ending Panel (Reuse Door UI)")]
    public GameObject endingCanvas;
    public TextMeshProUGUI endingText;
    [TextArea(3, 10)]
    public string declineEndingMessage =
        "ENDING 2/5\n\nYou ignored your best friend.\n" +
        "You never met the dog...\n" +
        "But something else found them...";

    [Header("Text Message UI")]
    public GameObject textMessagePanel;
    public TextMeshProUGUI contactNameText;
    public TextMeshProUGUI messageText;
    public GameObject messageAcceptButton;
    public GameObject messageDeclineButton;

    [Header("Text Message Settings")]
    public string contactName = "Bestie 🐾";
    [TextArea(3, 10)]
    public string dogSitMessage =
        "Hey!! Are you free after your shift? \n" +
        "I need someone to watch my dog tonight... \n" +
        "Please please please 🐶🙏";

    private Vector3 originalImageScale;
    private Vector3 originalImagePos;

    private System.Action onAccepted;
    private System.Action onDeclined;

  void Awake()
{
    Instance = this;
    
    originalImageScale = airdropImage.transform.localScale;
    originalImagePos = airdropImage.transform.localPosition;
    
    // CHANGE: Keep phonePanel active, but hide all children
    if (phonePanel != null) phonePanel.SetActive(true);
    
    // Hide all children at start
    if (actionButtons != null) actionButtons.SetActive(false);
    if (playerDialogueText != null) playerDialogueText.text = "";
    if (textMessagePanel != null) textMessagePanel.SetActive(false);
    if (airdropImage != null) airdropImage.gameObject.SetActive(false);
    
    // Hide the phone background/frame
    if (phoneCanvasGroup != null) phoneCanvasGroup.alpha = 0f;
}

    // ============================
    // AIRDROP LOGIC
    // ============================

    public void ReceiveAirdrop(string content, Sprite horrorPhoto = null)
    {
        StopAllCoroutines(); 

       if (textMessagePanel != null) textMessagePanel.SetActive(false); // Hide text UI
        actionButtons.SetActive(false); 

        airdropImage.transform.localScale = originalImageScale;
        airdropImage.transform.localPosition = originalImagePos;

        StartCoroutine(ProcessAirdrop(horrorPhoto));
    }

    IEnumerator ProcessAirdrop(Sprite horrorPhoto)
    {
        if (phoneCanvasGroup != null)
            phoneCanvasGroup.alpha = 1f;
        
        yield return new WaitForEndOfFrame(); 

        if (horrorPhoto != null)
        {
            airdropImage.sprite = horrorPhoto;
            airdropImage.gameObject.SetActive(true);
        }
        else
        {
            airdropImage.gameObject.SetActive(false);
        }

        if (notificationSound != null && audioSource != null)
            audioSource.PlayOneShot(notificationSound);

        if (actionButtons != null)
            actionButtons.SetActive(true);
    }

    public void OnAcceptPressed()
    {
        StartCoroutine(EnlargeImageAndTalk());
    }

    public void OnDeclinePressed()
    {
        ClosePhone();
    }

    IEnumerator EnlargeImageAndTalk()
    {
        if (actionButtons != null)
            actionButtons.SetActive(false); 

        float duration = 0.8f; 
        float elapsed = 0;
        
        Image phoneBackground = phonePanel.GetComponent<Image>();
        if (phoneBackground != null) phoneBackground.enabled = false;

        Vector3 targetScale = originalImageScale * 5.0f; 
        Vector3 targetPos = Vector3.zero; 
        
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            airdropImage.transform.localScale = Vector3.Lerp(originalImageScale, targetScale, t);
            airdropImage.transform.localPosition = Vector3.Lerp(originalImagePos, targetPos, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (audioSource != null && scaryZoomSound != null)
        {
            audioSource.PlayOneShot(scaryZoomSound);
        }

        if (playerDialogueText != null)
        {
            playerDialogueText.text = "Well that's creepy...";
            playerDialogueText.gameObject.SetActive(true); 
        }
        
        yield return new WaitForSeconds(3f);
        
        if (phoneBackground != null) phoneBackground.enabled = true;
        ClosePhone();
    }

   public void ClosePhone()
{
    if (playerDialogueText != null) playerDialogueText.text = "";

    airdropImage.transform.localScale = originalImageScale;
    airdropImage.transform.localPosition = originalImagePos;

    // CHANGE: Just hide via alpha instead of deactivating panel
    if (phoneCanvasGroup != null)
        phoneCanvasGroup.alpha = 0f;
    
    // Hide all children
    if (airdropImage != null) airdropImage.gameObject.SetActive(false);
    if (textMessagePanel != null) textMessagePanel.SetActive(false);
    if (actionButtons != null) actionButtons.SetActive(false);
}

    // ============================
    // TEXT MESSAGE SYSTEM
    // ============================

public void ReceiveTextMessage(
    System.Action onAccept = null,
    System.Action onDecline = null)
{
    onAccepted = onAccept;
    onDeclined = onDecline;

    StopAllCoroutines();

    // Phone panel already active, just swap children
    // FIRST: Hide airdrop UI
    if (airdropImage != null)
    {
        airdropImage.gameObject.SetActive(false);
        airdropImage.transform.localScale = originalImageScale;
        airdropImage.transform.localPosition = originalImagePos;
    }
    
    if (actionButtons != null)
        actionButtons.SetActive(false);
    
    if (playerDialogueText != null)
    {
        playerDialogueText.text = "";
        playerDialogueText.gameObject.SetActive(false);
    }

    // SECOND: Show text message UI
    if (textMessagePanel != null)
        textMessagePanel.SetActive(true);

    if (contactNameText != null)
        contactNameText.text = contactName;

    if (messageText != null)
        messageText.text = "";

    if (messageAcceptButton != null) messageAcceptButton.SetActive(false);
    if (messageDeclineButton != null) messageDeclineButton.SetActive(false);

    StartCoroutine(ShowTextMessage());
}

  IEnumerator ShowTextMessage()
{
    // Play notification sound
    if (textMessageSound != null && audioSource != null)
        audioSource.PlayOneShot(textMessageSound);

    yield return new WaitForSeconds(0.5f);

    // Show phone instantly (don't fade the whole thing)
    if (phoneCanvasGroup != null)
    {
        phoneCanvasGroup.alpha = 1f; // Just set to 1 instantly
    }

    // 3) Optional: second "UI blip" when the message text appears
    if (notificationSound != null && audioSource != null)
        audioSource.PlayOneShot(notificationSound);

    // Clear and reset scroll
    if (messageText != null)
        messageText.text = "";

    ScrollRect scrollRect = textMessagePanel.GetComponentInChildren<ScrollRect>();
    if (scrollRect != null)
    {
        scrollRect.verticalNormalizedPosition = 1f;
    }

    // Show message instantly
    if (messageText != null)
        messageText.text = dogSitMessage;

    yield return new WaitForSeconds(0.5f);

    // Show buttons
    if (messageAcceptButton != null) messageAcceptButton.SetActive(true);
    if (messageDeclineButton != null) messageDeclineButton.SetActive(true);
}

void ShowMessage(string message, bool addNewlineBefore = true)
{
    if (messageText == null) return;

    if (addNewlineBefore)
    {
        messageText.text += "\n\n";
    }

    messageText.text += message;

    // Scroll stays at top initially, only scroll down when content exceeds viewport
    Canvas.ForceUpdateCanvases();
    
    ScrollRect scrollRect = textMessagePanel.GetComponentInChildren<ScrollRect>();
    if (scrollRect != null)
    {
        // Check if content is taller than viewport
        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;
        
        if (content.rect.height > viewport.rect.height)
        {
            // Content overflows, scroll to bottom
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
    public void OnTextMessageAccepted()
    {
        StartCoroutine(HandleTextMessageAccepted());
    }

    public void OnTextMessageDeclined()
    {
        StartCoroutine(HandleTextMessageDeclined());
    }

    IEnumerator HandleTextMessageAccepted()
    {
        // Hide buttons
        if (messageAcceptButton != null) messageAcceptButton.SetActive(false);
        if (messageDeclineButton != null) messageDeclineButton.SetActive(false);

        // Player typing delay
        yield return new WaitForSeconds(0.8f);

        // Show player reply
        ShowMessage("You: Sure! I'll be there after my shift 🐾", true);

        // Friend typing delay
        yield return new WaitForSeconds(1.2f);

        // Show friend response
        ShowMessage("Bestie: YAYY thank you!! You're the best!! 🎉", true);

        yield return new WaitForSeconds(2f);

        CloseTextMessage();
        onAccepted?.Invoke();
    }

    IEnumerator HandleTextMessageDeclined()
    {
        // Hide buttons
        if (messageAcceptButton != null) messageAcceptButton.SetActive(false);
        if (messageDeclineButton != null) messageDeclineButton.SetActive(false);

        // Player typing delay
        yield return new WaitForSeconds(0.8f);

        // Show player reply
        ShowMessage("You: Sorry, can't tonight!", true);

        // Friend typing delay
        yield return new WaitForSeconds(1.2f);

        // Show friend response
        ShowMessage("Bestie: Aw no worries 😢", true);

        yield return new WaitForSeconds(2f);

        ShowDeclineEnding();
        onDeclined?.Invoke();
    }

    void CloseTextMessage()
    {
        if (textMessagePanel != null) textMessagePanel.SetActive(false);
        if (airdropImage != null) airdropImage.gameObject.SetActive(true);

        ClosePhone();
    }

    // ============================
    // ENDING 2/5
    // ============================

    public void ShowDeclineEnding()
    {
        ClosePhone();

        if (endingCanvas != null)
        {
            endingCanvas.SetActive(true);

            Transform panel = endingCanvas.transform.Find("EndingPanel");
            if (panel != null) panel.gameObject.SetActive(true);

            if (endingText != null)
            {
                endingText.text = declineEndingMessage;
            }
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}