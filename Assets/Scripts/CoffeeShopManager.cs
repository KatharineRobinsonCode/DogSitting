using UnityEngine;

/// <summary>
/// Manages the coffee shop scene flow and task progression.
/// Coordinates between TaskManager, CustomerQueue, and player actions.
/// </summary>
public class CoffeeShopManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("References")]
    [Tooltip("The customer queue manager")]
    [SerializeField] private CustomerQueue customerQueue;
    
    [Tooltip("Trigger area behind the counter")]
    [SerializeField] private Collider counterAreaTrigger;
    
    [Header("Settings")]
    [Tooltip("Tag used to identify the player")]
    [SerializeField] private string playerTag = "Player";
    
    #endregion
    
    #region Private Fields
    
    private bool hasEnteredCounter = false;
    private bool allCustomersServed = false;
    
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
        // Show initial task
        TaskManager.Instance?.ShowTask("Go behind counter");
        
        ValidateReferences();
    }
    
    private void ValidateReferences()
    {
        if (customerQueue == null)
        {
customerQueue = FindFirstObjectByType<CustomerQueue>();
            
            if (customerQueue == null)
            {
                Debug.LogError("[CoffeeShopManager] CustomerQueue not found!");
            }
        }
    }
    
    #endregion
    
    #region Player Events
    
    /// <summary>
    /// Called when player enters the counter area trigger.
    /// Can be called via OnTriggerEnter or manually.
    /// </summary>
    public void OnPlayerEnteredCounterArea()
    {
        if (hasEnteredCounter)
        {
            return;
        }
        
        hasEnteredCounter = true;
        
        Debug.Log("[CoffeeShopManager] Player entered counter area");
        
        // Show feedback - FIXED
        FeedbackManager.Instance?.ShowMessage("Ready to serve customers!", FeedbackManager.MessageType.Success);
        
        // Update task - customers will update it with specific orders
        TaskManager.Instance?.ShowTask("Serve customers");
        
        // CustomerQueue handles its own serving in Start() - no need to call StartServing()
    }
    
    /// <summary>
    /// Called by CustomerQueue or other scripts when all customers are done.
    /// </summary>
    public void OnAllCustomersServed()
    {
        if (allCustomersServed)
        {
            return;
        }
        
        allCustomersServed = true;
        
        Debug.Log("[CoffeeShopManager] All customers served!");
        
        // Show final task
        TaskManager.Instance?.ShowTask("Leave cafe");
        
        // Optional: Show feedback - FIXED
        FeedbackManager.Instance?.ShowMessage("Shift complete! You can leave now.", FeedbackManager.MessageType.Success);
    }
    
    #endregion
    
    #region Trigger Detection
    
    // If using a trigger collider behind counter
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            OnPlayerEnteredCounterArea();
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Manually trigger the counter entrance (if not using colliders).
    /// </summary>
    public void TriggerCounterEntrance()
    {
        OnPlayerEnteredCounterArea();
    }
    
    #endregion
}