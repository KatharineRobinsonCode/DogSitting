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
[SerializeField] private DialogueRunner dialogueRunner;
private bool toiletDialogueTriggered = false;

    private bool hasEnteredCounter = false;
    private bool allCustomersServed = false;
    private int dirtSpotsRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

   private void Start()
{
    dirtSpotsRemaining = totalDirtSpots;
    TaskManager.Instance?.ShowTask("Go behind counter");

    if (customerQueue == null)
        customerQueue = FindFirstObjectByType<CustomerQueue>();

    if (dialogueRunner == null)
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
}
private void Update()
{
    if (!toiletDialogueTriggered && 
        TaskManager.Instance != null &&
        TaskManager.Instance.IsCurrentTask("Check on the customer in the toilet"))
    {
        toiletDialogueTriggered = true;
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            dialogueRunner.StartDialogue("ToiletThought");
    }
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
        if (allCustomersServed) return;
        allCustomersServed = true;

        foreach (NpcCustomer seated in seatedCustomers)
        {
            if (seated != null)
                seated.ForceLeave();
        }

        foreach (DirtSpot spot in dirtSpots)
        {
            if (spot != null)
                spot.Activate();
        }

        TaskManager.Instance?.ShowTask("Sweep the floor");
        FeedbackManager.Instance?.ShowMessage("Shift almost over - sweep up!", FeedbackManager.MessageType.Success);
    }

    public void OnDirtSpotCleaned()
    {
        dirtSpotsRemaining--;

        if (dirtSpotsRemaining <= 0)
        {
            TaskManager.Instance?.ShowTask("Leave pub");
            FeedbackManager.Instance?.ShowMessage("All clean! Head to the door.", FeedbackManager.MessageType.Success);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            OnPlayerEnteredCounterArea();
    }

    public void TriggerCounterEntrance() => OnPlayerEnteredCounterArea();
}