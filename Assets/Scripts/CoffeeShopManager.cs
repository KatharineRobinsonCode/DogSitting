using UnityEngine;
using Yarn.Unity;


public class CoffeeShopManager : MonoBehaviour
{
    public static CoffeeShopManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CustomerQueue customerQueue;
    [SerializeField] private string playerTag = "Player";

    [Header("End of Shift")]
    [SerializeField] private NpcCustomer[] seatedCustomers;
    [SerializeField] private int totalDirtSpots = 3;
    [SerializeField] private DirtSpot[] dirtSpots;

    [Header("Toilet Event")]
    [SerializeField] private AudioSource cryingAudio;

    [Header("Dialogue")]
 private DialogueRunner dialogueRunner;
private bool toiletDialogueTriggered = false;

    private bool hasEnteredCounter = false;
    private bool allCustomersServed = false;
    private int dirtSpotsRemaining;

    [Header("Last Call")]
[SerializeField] private AudioSource lastCallAudio;
[SerializeField] private AudioClip lastCallClip;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

   private void Start()
{
    dirtSpotsRemaining = totalDirtSpots;
    TaskManager.Instance?.ShowTask("Go behind the bar");
    if (customerQueue == null)
        customerQueue = FindFirstObjectByType<CustomerQueue>();

    if (dialogueRunner == null)
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            Debug.Log($"[CoffeeShopManager] DialogueRunner found: {dialogueRunner != null}");

}
private void Update()
{
    if (!toiletDialogueTriggered && 
        TaskManager.Instance != null &&
        TaskManager.Instance.IsCurrentTask("Check on the customer in the toilet"))
    {
        toiletDialogueTriggered = true;

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            // Set up canvas same way NpcCustomer does
            Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
            if (canvasComponent != null)
            {
                canvasComponent.gameObject.SetActive(true);
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasGroup group = canvasComponent.gameObject.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
            }

            dialogueRunner.StartDialogue("ToiletThought");
        }
    }
}
private void OnReadyForLeavePub()
{
    dialogueRunner.onDialogueComplete.RemoveListener(OnReadyForLeavePub);
    StartLeavePubDialogue();
}

private void StartLeavePubDialogue()
{
    Debug.Log("[CoffeeShopManager] StartLeavePubDialogue called");
    
    Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
    if (canvasComponent != null)
    {
        canvasComponent.gameObject.SetActive(true);
        canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasGroup group = canvasComponent.gameObject.GetComponent<CanvasGroup>();
        if (group != null) group.alpha = 1f;
    }

    Debug.Log($"[CoffeeShopManager] Starting node: LeavePub, canvas null: {canvasComponent == null}");
    dialogueRunner.StartDialogue("LeavePub");
}
    public void OnPlayerEnteredCounterArea()
    {
        if (hasEnteredCounter) return;
        hasEnteredCounter = true;

        FeedbackManager.Instance?.ShowMessage("Ready to serve customers!", FeedbackManager.MessageType.Success);
        TaskManager.Instance?.ShowTask("Serve customers");
    }

public void OnThirdCustomerServed()
{
    TaskManager.Instance?.ShowTask("Check on the customer in the toilet");
}

    public void OnToiletTaskComplete()
    {
        TaskManager.Instance?.ShowTask("Serve customers");
        FeedbackManager.Instance?.ShowMessage("Back to it.", FeedbackManager.MessageType.Success);
        customerQueue?.ResumeQueue();
    }

  public void OnAllCustomersServed()
{
    Debug.Log("[CoffeeShopManager] OnAllCustomersServed called");
    if (allCustomersServed) return;
    allCustomersServed = true;

    // Force all seated customers to leave
    foreach (NpcCustomer seated in seatedCustomers)
    {
        if (seated != null)
            seated.ForceLeave();
    }

    // Play last call bell immediately
    if (lastCallAudio != null && lastCallClip != null)
        lastCallAudio.PlayOneShot(lastCallClip);

    // New task — clean bathroom first
    TaskManager.Instance?.ShowTask("Clean bathroom");
    FeedbackManager.Instance?.ShowMessage("Last orders! Clean the bathroom before you go.", FeedbackManager.MessageType.Success);
}
public void OnBathroomCleaningComplete()
{
    // Now activate the pub dirt spots
    foreach (DirtSpot spot in dirtSpots)
    {
        if (spot != null)
            spot.Activate();
    }

    TaskManager.Instance?.ShowTask($"Sweep the floor (0/{totalDirtSpots})");
}
public void OnDirtSpotCleaned()
{
    dirtSpotsRemaining--;
    int cleaned = totalDirtSpots - dirtSpotsRemaining;
    Debug.Log($"[CoffeeShopManager] Pub dirt spot cleaned: {cleaned}/{totalDirtSpots}");
    
    if (cleaned == 2)
    {
        Debug.Log("[CoffeeShopManager] Triggering glass smash!");
        GlassSmashEvent glassSmash = FindFirstObjectByType<GlassSmashEvent>();
        if (glassSmash != null) glassSmash.TriggerGlassSmash();
    }
    if (dirtSpotsRemaining <= 0)
    {
        TaskManager.Instance?.ShowTask("Leave pub");

        Debug.Log($"[CoffeeShopManager] Starting leave dialogue, isRunning: {dialogueRunner?.IsDialogueRunning}");

        if (dialogueRunner != null)
        {
            if (!dialogueRunner.IsDialogueRunning)
                StartLeavePubDialogue();
            else
                dialogueRunner.onDialogueComplete.AddListener(OnReadyForLeavePub);
        }
        else
    {
        // Update task with counter
        TaskManager.Instance?.ShowTask($"Sweep the floor ({cleaned}/{totalDirtSpots})");
    }
}
}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            OnPlayerEnteredCounterArea();
    }

    public void TriggerCounterEntrance() => OnPlayerEnteredCounterArea();
}