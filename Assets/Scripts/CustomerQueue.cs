using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the queue of NPC customers waiting to be served.
/// Handles calling customers to the counter and triggering story events at specific milestones.
/// </summary>
public class CustomerQueue : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Customer Queue")]
    [Tooltip("List of customers in order (first in list is first to be called)")]
    [SerializeField] private List<NpcCustomer> customersInShop = new List<NpcCustomer>();
    
    [Header("Story Events")]
    [Tooltip("Number of customers that must be served before triggering dog sitting text")]
    [SerializeField] private int customersBeforeTextMessage = 5;
    
    [Header("Debug")]
    [Tooltip("Enable detailed queue logging")]
    [SerializeField] private bool enableDebugLogs = true;
    
    #endregion
    
    #region Private Fields
    
    private int customersServed = 0;
    private bool hasTriggeredTextMessage = false;
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Returns the number of customers currently in the queue.
    /// </summary>
    public int CustomersInQueue => customersInShop.Count;
    
    /// <summary>
    /// Returns the total number of customers served.
    /// </summary>
    public int CustomersServed => customersServed;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeQueue();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeQueue()
    {
        LogDebug($"[CustomerQueue] Starting with {customersInShop.Count} customers");
        
        if (HasCustomersWaiting())
        {
            CallNextCustomer();
        }
        else
        {
            LogDebug("[CustomerQueue] No customers in queue at start");
        }
    }
    
    #endregion
    
    #region Queue Management
    
    /// <summary>
    /// Called when a customer has been served and is leaving.
    /// Removes them from queue and calls the next customer.
    /// </summary>
    /// <param name="customer">The customer who is leaving</param>
    public void CustomerLeft(NpcCustomer customer)
    {
        if (customer == null)
        {
            Debug.LogError("[CustomerQueue] CustomerLeft called with null customer!");
            return;
        }
        
        LogDebug($"[CustomerQueue] Customer left: {customer.name}");
        
        IncrementCustomersServed();
        RemoveCustomerFromQueue(customer);
        
        // Check for story events before calling next customer
        if (ShouldTriggerTextMessage())
        {
            TriggerTextMessage();
            return; // Text message will call next customer after player responds
        }
        
        // Normal flow - call next customer
        CallNextCustomerIfAvailable();
    }
    
    private void IncrementCustomersServed()
    {
        customersServed++;
        LogDebug($"[CustomerQueue] Total customers served: {customersServed}");
    }
    
    private void RemoveCustomerFromQueue(NpcCustomer customer)
    {
        if (customersInShop.Contains(customer))
        {
            customersInShop.Remove(customer);
            LogDebug($"[CustomerQueue] Removed {customer.name}. Remaining: {customersInShop.Count}");
        }
        else
        {
            Debug.LogWarning($"[CustomerQueue] {customer.name} was not in queue!");
        }
    }
    
    private void CallNextCustomerIfAvailable()
    {
        if (HasCustomersWaiting())
        {
            CallNextCustomer();
        }
        else
        {
            LogDebug("[CustomerQueue] No more customers waiting");
        }
    }
    
    private void CallNextCustomer()
    {
        if (!HasCustomersWaiting())
        {
            return;
        }
        
        NpcCustomer nextCustomer = customersInShop[0];
        LogDebug($"[CustomerQueue] Calling next customer: {nextCustomer.name}");
        nextCustomer.CallToCounter();
    }
    
    private bool HasCustomersWaiting()
    {
        return customersInShop.Count > 0;
    }
    
    #endregion
    
    #region Story Events - Text Message
    
    private bool ShouldTriggerTextMessage()
    {
        return !hasTriggeredTextMessage && 
               customersServed == customersBeforeTextMessage;
    }
    
    private void TriggerTextMessage()
    {
        hasTriggeredTextMessage = true;
        
        LogDebug($"[CustomerQueue] Triggering dog sitting text after {customersBeforeTextMessage} customers");
        
        if (!ValidatePhoneManager())
        {
            HandlePhoneManagerMissing();
            return;
        }
        
        ShowDogSitTextMessage();
    }
    
    private bool ValidatePhoneManager()
    {
        if (PhoneManager.Instance != null)
        {
            return true;
        }
        
        Debug.LogError("[CustomerQueue] PhoneManager.Instance is NULL!");
        return false;
    }
    
    private void HandlePhoneManagerMissing()
    {
        Debug.LogWarning("[CustomerQueue] Skipping text message event, resuming queue");
        CallNextCustomerIfAvailable();
    }
    
    private void ShowDogSitTextMessage()
    {
        PhoneManager.Instance.ReceiveTextMessage(
            onAccept: HandleDogSitAccepted,
            onDecline: HandleDogSitDeclined
        );
    }
    
    private void HandleDogSitAccepted()
    {
        LogDebug("[CustomerQueue] Player accepted dog sitting");
        ResumeQueueAfterPhone();
    }
    
    private void HandleDogSitDeclined()
    {
        LogDebug("[CustomerQueue] Player declined dog sitting - Bad ending triggered");
        // PhoneManager handles ending display
        // Queue ends here as game is over
    }
    
    private void ResumeQueueAfterPhone()
    {
        LogDebug("[CustomerQueue] Resuming queue after phone interaction");
        CallNextCustomerIfAvailable();
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Manually adds a customer to the end of the queue.
    /// </summary>
    public void AddCustomer(NpcCustomer customer)
    {
        if (customer == null)
        {
            Debug.LogWarning("[CustomerQueue] Attempted to add null customer");
            return;
        }
        
        if (customersInShop.Contains(customer))
        {
            Debug.LogWarning($"[CustomerQueue] {customer.name} is already in queue");
            return;
        }
        
        customersInShop.Add(customer);
        LogDebug($"[CustomerQueue] Added {customer.name}. Total in queue: {customersInShop.Count}");
        
        // If this is the only customer, call them immediately
        if (customersInShop.Count == 1)
        {
            CallNextCustomer();
        }
    }
    
    /// <summary>
    /// Clears all customers from the queue (useful for testing/resetting).
    /// </summary>
    public void ClearQueue()
    {
        int count = customersInShop.Count;
        customersInShop.Clear();
        LogDebug($"[CustomerQueue] Cleared {count} customers from queue");
    }
    
    /// <summary>
    /// Returns the customer currently at the front of the queue (next to be called).
    /// </summary>
    public NpcCustomer GetNextCustomer()
    {
        return HasCustomersWaiting() ? customersInShop[0] : null;
    }
    
    /// <summary>
    /// Checks if a specific customer is in the queue.
    /// </summary>
    public bool IsCustomerInQueue(NpcCustomer customer)
    {
        return customer != null && customersInShop.Contains(customer);
    }
    
    /// <summary>
    /// Returns the position of a customer in the queue (0 = next, -1 = not in queue).
    /// </summary>
    public int GetCustomerPosition(NpcCustomer customer)
    {
        return customer != null ? customersInShop.IndexOf(customer) : -1;
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
    
    /// <summary>
    /// Logs the current state of the queue (for debugging).
    /// </summary>
    [ContextMenu("Log Queue State")]
    public void LogQueueState()
    {
        Debug.Log("=== QUEUE STATE ===");
        Debug.Log($"Customers Served: {customersServed}");
        Debug.Log($"Customers in Queue: {customersInShop.Count}");
        Debug.Log($"Text Message Triggered: {hasTriggeredTextMessage}");
        
        for (int i = 0; i < customersInShop.Count; i++)
        {
            NpcCustomer customer = customersInShop[i];
            string status = i == 0 ? "(ACTIVE)" : $"(Position {i})";
            Debug.Log($"  {customer.name} {status}");
        }
        
        Debug.Log("==================");
    }
    
    #endregion
}