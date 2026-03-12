using UnityEngine;

public class CoffeeShopManager : MonoBehaviour
{
    public static CoffeeShopManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CustomerQueue customerQueue;
    [SerializeField] private string playerTag = "Player";

    [Header("End of Shift")]
    [SerializeField] private NpcCustomer[] seatedCustomers;
    [SerializeField] private int totalDirtSpots = 3;

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
    }

    public void OnPlayerEnteredCounterArea()
    {
        if (hasEnteredCounter) return;
        hasEnteredCounter = true;

        Debug.Log("[CoffeeShopManager] Player entered counter area");
        FeedbackManager.Instance?.ShowMessage("Ready to serve customers!", FeedbackManager.MessageType.Success);
        TaskManager.Instance?.ShowTask("Serve customers");
    }

    public void OnAllCustomersServed()
    {
        if (allCustomersServed) return;
        allCustomersServed = true;

        Debug.Log("[CoffeeShopManager] All customers served!");

        // Send seated customers home
        foreach (NpcCustomer seated in seatedCustomers)
        {
            if (seated != null)
                seated.FinishOrderAndLeave();
        }

        TaskManager.Instance?.ShowTask("Sweep the floor");
        FeedbackManager.Instance?.ShowMessage("Shift almost over - sweep up!", FeedbackManager.MessageType.Success);
    }

   public void OnDirtSpotCleaned()
{
    dirtSpotsRemaining--;
    Debug.Log($"[CoffeeShopManager] Dirt spot cleaned. Remaining: {dirtSpotsRemaining}");

    if (dirtSpotsRemaining <= 0)
    {
        TaskManager.Instance?.ShowTask("End shift");
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