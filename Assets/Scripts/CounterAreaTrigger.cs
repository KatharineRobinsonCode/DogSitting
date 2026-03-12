using UnityEngine;

/// <summary>
/// Triggers the coffee shop manager when player enters counter area.
/// Attach to a trigger collider behind the counter.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CounterAreaTrigger : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Settings")]
    [Tooltip("Tag used to identify the player")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;
    
    #endregion
    
    #region Private Fields
    
    private bool hasTriggered = false;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        ValidateSetup();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        LogDebug($"[CounterTrigger] Something entered: {other.name} (Tag: {other.tag})");
        
        if (hasTriggered)
        {
            LogDebug("[CounterTrigger] Already triggered, ignoring");
            return;
        }
        
        if (other.CompareTag(playerTag))
        {
            LogDebug("[CounterTrigger] PLAYER DETECTED! Triggering counter entrance");
            TriggerCounterEntrance();
        }
        else
        {
            LogDebug($"[CounterTrigger] Not the player (expected tag: {playerTag})");
        }
    }
    
    #endregion
    
    #region Trigger Logic
    
    private void TriggerCounterEntrance()
    {
        hasTriggered = true;
        
        CoffeeShopManager manager = FindObjectOfType<CoffeeShopManager>();
        
        if (manager != null)
        {
            LogDebug("[CounterTrigger] Found CoffeeShopManager, calling OnPlayerEnteredCounterArea");
            manager.OnPlayerEnteredCounterArea();
        }
        else
        {
            Debug.LogError("[CounterTrigger] CoffeeShopManager not found in scene!");
        }
    }
    
    #endregion
    
    #region Validation
    
    private void ValidateSetup()
    {
        Collider col = GetComponent<Collider>();
        
        if (col == null)
        {
            Debug.LogError("[CounterTrigger] No Collider found on this GameObject!", this);
            return;
        }
        
        if (!col.isTrigger)
        {
            Debug.LogWarning("[CounterTrigger] Collider is not set as trigger! Fixing automatically.", this);
            col.isTrigger = true;
        }
        
        LogDebug("[CounterTrigger] Setup validated successfully");
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
    
    #endregion
}