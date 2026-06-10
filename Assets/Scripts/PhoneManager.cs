using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Yarn.Unity;

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

    [Header("Ending Panel")]
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
    "Hello, are you free watch my dog tonight. \n";
    [Header("Carol Text Settings")]
    public string carolContactName = "Carol 🏠";
    [TextArea(3, 10)]
    public string carolCheckInMessage = "Hey! Just arrived at yours — Brinkley is safe and sound 🐶";

    [Header("Pizza Order UI")]
    public GameObject pizzaOrderPanel;
    public GameObject pizzaConfirmPanel;
    public TextMeshProUGUI pizzaConfirmText;

    private Vector3 originalImageScale;
    private Vector3 originalImagePos;
    private System.Action onAccepted;
    private System.Action onDeclined;

    void Awake()
    {
        Instance = this;
        
        originalImageScale = airdropImage.transform.localScale;
        originalImagePos = airdropImage.transform.localPosition;
        
        if (phonePanel != null) phonePanel.SetActive(true);
        if (actionButtons != null) actionButtons.SetActive(false);
        if (playerDialogueText != null) playerDialogueText.text = "";
        if (textMessagePanel != null) textMessagePanel.SetActive(false);
        if (airdropImage != null) airdropImage.gameObject.SetActive(false);
        if (phoneCanvasGroup != null) phoneCanvasGroup.alpha = 0f;
    }

    // ============================
    // SHARED PHONE OPEN/CLOSE
    // ============================

    private void OpenPhone()
    {
        if (phoneCanvasGroup != null)
            phoneCanvasGroup.alpha = 1f;

        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowCursorPublic();
    }

    public void ClosePhone()
    {
        if (playerDialogueText != null) playerDialogueText.text = "";

        airdropImage.transform.localScale = originalImageScale;
        airdropImage.transform.localPosition = originalImagePos;

        if (phoneCanvasGroup != null) phoneCanvasGroup.alpha = 0f;
        if (airdropImage != null) airdropImage.gameObject.SetActive(false);
        if (textMessagePanel != null) textMessagePanel.SetActive(false);
        if (actionButtons != null) actionButtons.SetActive(false);
        if (pizzaOrderPanel != null) pizzaOrderPanel.SetActive(false);
        if (pizzaConfirmPanel != null) pizzaConfirmPanel.SetActive(false);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }

    // ============================
    // AIRDROP LOGIC
    // ============================

    public void ReceiveAirdrop(string content, Sprite horrorPhoto = null)
    {
        StopAllCoroutines();

        if (textMessagePanel != null) textMessagePanel.SetActive(false);
        if (actionButtons != null) actionButtons.SetActive(false);

        airdropImage.transform.localScale = originalImageScale;
        airdropImage.transform.localPosition = originalImagePos;

        StartCoroutine(ProcessAirdrop(horrorPhoto));
    }

    IEnumerator ProcessAirdrop(Sprite horrorPhoto)
    {
        yield return new WaitForEndOfFrame();

        if (horrorPhoto != null)
        {
            airdropImage.sprite = horrorPhoto;
            airdropImage.gameObject.SetActive(false);
        }

        if (notificationSound != null && audioSource != null)
            audioSource.PlayOneShot(notificationSound);

        if (actionButtons != null)
            actionButtons.SetActive(true);

        OpenPhone();
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

 airdropImage.gameObject.SetActive(true); 

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
            audioSource.PlayOneShot(scaryZoomSound);

        if (playerDialogueText != null)
        {
            playerDialogueText.text = "Well that's creepy...";
            playerDialogueText.gameObject.SetActive(true);
        }
        
        yield return new WaitForSeconds(3f);
        
        if (phoneBackground != null) phoneBackground.enabled = true;
        ClosePhone();
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

        if (airdropImage != null)
        {
            airdropImage.gameObject.SetActive(false);
            airdropImage.transform.localScale = originalImageScale;
            airdropImage.transform.localPosition = originalImagePos;
        }
        
        if (actionButtons != null) actionButtons.SetActive(false);
        if (playerDialogueText != null)
        {
            playerDialogueText.text = "";
            playerDialogueText.gameObject.SetActive(false);
        }

        if (textMessagePanel != null) textMessagePanel.SetActive(true);
        if (contactNameText != null) contactNameText.text = contactName;
        if (messageText != null) messageText.text = "";
        if (messageAcceptButton != null) messageAcceptButton.SetActive(false);
        if (messageDeclineButton != null) messageDeclineButton.SetActive(false);

        StartCoroutine(ShowTextMessage());
    }

    IEnumerator ShowTextMessage()
    {
        if (textMessageSound != null && audioSource != null)
            audioSource.PlayOneShot(textMessageSound);

        yield return new WaitForSeconds(0.5f);

        OpenPhone();

        if (notificationSound != null && audioSource != null)
            audioSource.PlayOneShot(notificationSound);

        if (messageText != null) messageText.text = "";

        ScrollRect scrollRect = textMessagePanel.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;

        if (messageText != null)
            messageText.text = dogSitMessage;

        yield return new WaitForSeconds(0.5f);

        if (messageAcceptButton != null) messageAcceptButton.SetActive(true);
        if (messageDeclineButton != null) messageDeclineButton.SetActive(true);
    }

    void ShowMessage(string message, bool addNewlineBefore = true)
    {
        if (messageText == null) return;

        if (addNewlineBefore)
            messageText.text += "\n\n";

        messageText.text += message;

        Canvas.ForceUpdateCanvases();
        
        ScrollRect scrollRect = textMessagePanel.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            RectTransform content = scrollRect.content;
            RectTransform viewport = scrollRect.viewport;
            
            if (content.rect.height > viewport.rect.height)
                scrollRect.verticalNormalizedPosition = 0f;
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
        if (messageAcceptButton != null) messageAcceptButton.SetActive(false);
        if (messageDeclineButton != null) messageDeclineButton.SetActive(false);

        yield return new WaitForSeconds(0.8f);
        ShowMessage("<color=#006400><align=right>Sure! I'll be there after my shift", true);

        yield return new WaitForSeconds(1.2f);
        ShowMessage("<color=#8B008B><align=left>Thanks, key will be in the plant pot outside the flat. Brinkley will be inside waiting.", true);

        yield return new WaitForSeconds(4f);

        CloseTextMessage();
        onAccepted?.Invoke();
    }

    IEnumerator HandleTextMessageDeclined()
    {
        if (messageAcceptButton != null) messageAcceptButton.SetActive(false);
        if (messageDeclineButton != null) messageDeclineButton.SetActive(false);

        yield return new WaitForSeconds(0.8f);
        ShowMessage("You: Sorry, can't tonight!", true);

        yield return new WaitForSeconds(1.2f);
        ShowMessage("Hmm... no worries", true);

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
    // CAROL CHECK-IN
    // ============================

    public void ReceiveCarolCheckIn(System.Action onSent = null)
    {
        onAccepted = onSent;
        onDeclined = null;

        StopAllCoroutines();

        if (airdropImage != null)
        {
            airdropImage.gameObject.SetActive(false);
            airdropImage.transform.localScale = originalImageScale;
            airdropImage.transform.localPosition = originalImagePos;
        }

        if (actionButtons != null) actionButtons.SetActive(false);
        if (playerDialogueText != null)
        {
            playerDialogueText.text = "";
            playerDialogueText.gameObject.SetActive(false);
        }

        if (textMessagePanel != null) textMessagePanel.SetActive(true);
        if (contactNameText != null) contactNameText.text = carolContactName;
        if (messageText != null) messageText.text = "";
        if (messageAcceptButton != null) messageAcceptButton.SetActive(false);
        if (messageDeclineButton != null) messageDeclineButton.SetActive(false);

        StartCoroutine(ShowCarolCheckIn());
    }

    IEnumerator ShowCarolCheckIn()
    {
        if (textMessageSound != null && audioSource != null)
            audioSource.PlayOneShot(textMessageSound);

        yield return new WaitForSeconds(0.5f);

        OpenPhone();

        if (notificationSound != null && audioSource != null)
            audioSource.PlayOneShot(notificationSound);

        if (messageText != null) messageText.text = carolCheckInMessage;

        yield return new WaitForSeconds(0.5f);

        if (messageAcceptButton != null) messageAcceptButton.SetActive(true);
    }

    public void OnCarolCheckInSent()
    {
        StartCoroutine(HandleCarolCheckInSent());
    }

    IEnumerator HandleCarolCheckInSent()
    {
        if (messageAcceptButton != null) messageAcceptButton.SetActive(false);

        yield return new WaitForSeconds(0.8f);
        ShowMessage("You: Here with Brinkley!", true);

        yield return new WaitForSeconds(2.5f);

        CloseTextMessage();

        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask();

        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();
        if (runner != null)
        {
            runner.onDialogueComplete.AddListener(OnPizzaThoughtComplete);
            runner.StartDialogue("PizzaThought");
        }
    }

    private void OnPizzaThoughtComplete()
    {
        DialogueRunner runner = FindFirstObjectByType<DialogueRunner>();
        if (runner != null)
            runner.onDialogueComplete.RemoveListener(OnPizzaThoughtComplete);

        OpenPizzaOrder();
    }

    // ============================
    // PIZZA ORDER SYSTEM
    // ============================

    public void OpenPizzaOrder()
    {
        StopAllCoroutines();

        if (textMessagePanel != null) textMessagePanel.SetActive(false);
        if (pizzaConfirmPanel != null) pizzaConfirmPanel.SetActive(false);
        if (pizzaOrderPanel != null) pizzaOrderPanel.SetActive(true);

        OpenPhone();
    }

    public void OnPizzaSelected(string pizzaName)
    {
        StartCoroutine(HandlePizzaOrder(pizzaName));
    }

    IEnumerator HandlePizzaOrder(string pizzaName)
    {
        if (pizzaOrderPanel != null) pizzaOrderPanel.SetActive(false);
        if (pizzaConfirmPanel != null) pizzaConfirmPanel.SetActive(true);

        if (pizzaConfirmText != null)
            pizzaConfirmText.text = $"🍕 {pizzaName}\n\nYour order is on its way!\nEstimated delivery: 30 mins";

        if (notificationSound != null && audioSource != null)
            audioSource.PlayOneShot(notificationSound);

        yield return new WaitForSeconds(3f);

        if (pizzaConfirmPanel != null) pizzaConfirmPanel.SetActive(false);

        ClosePhone();

        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask();

        Debug.Log("[PhoneManager] Pizza ordered, finding FoodBowl");
        FoodBowl foodBowl = FindFirstObjectByType<FoodBowl>();
        Debug.Log("[PhoneManager] FoodBowl found: " + (foodBowl != null));
        if (foodBowl != null)
            foodBowl.OnPizzaOrdered();
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
                endingText.text = declineEndingMessage;
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}