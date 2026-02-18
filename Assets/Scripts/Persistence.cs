using UnityEngine;

/// <summary>
/// Makes a GameObject persist across scene loads.
/// Attach this to objects that should not be destroyed when loading new scenes.
/// Automatically handles duplicates to prevent multiple instances.
/// </summary>
public class Persistence : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Persistence Settings")]
    [Tooltip("Unique identifier for this persistent object")]
    [SerializeField] private string persistenceID = "";
    
    [Tooltip("Allow multiple instances of this object across scenes")]
    [SerializeField] private bool allowMultipleInstances = false;
    
    [Header("Debug")]
    [Tooltip("Log when object persists or is destroyed")]
    [SerializeField] private bool enableDebugLogs = false;
    
    #endregion
    
    #region Private Fields
    
    private static System.Collections.Generic.HashSet<string> existingInstances = 
        new System.Collections.Generic.HashSet<string>();
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        if (ShouldPersist())
        {
            MakePersistent();
        }
        else
        {
            DestroyDuplicate();
        }
    }
    
    private void OnDestroy()
    {
        // Clean up registration when destroyed
        if (!string.IsNullOrEmpty(persistenceID))
        {
            existingInstances.Remove(persistenceID);
        }
    }
    
    #endregion
    
    #region Persistence Logic
    
    private bool ShouldPersist()
    {
        // Always allow if multiple instances permitted
        if (allowMultipleInstances)
        {
            return true;
        }
        
        // If no ID specified, generate one from object name
        if (string.IsNullOrEmpty(persistenceID))
        {
            persistenceID = gameObject.name;
        }
        
        // Check if an instance with this ID already exists
        if (existingInstances.Contains(persistenceID))
        {
            LogDebug($"[Persistence] Duplicate found: {persistenceID}");
            return false;
        }
        
        return true;
    }
    
    private void MakePersistent()
    {
        DontDestroyOnLoad(gameObject);
        
        // Register this instance
        if (!string.IsNullOrEmpty(persistenceID) && !allowMultipleInstances)
        {
            existingInstances.Add(persistenceID);
        }
        
        LogDebug($"[Persistence] Object persisted: {gameObject.name} (ID: {persistenceID})");
    }
    
    private void DestroyDuplicate()
    {
        LogDebug($"[Persistence] Destroying duplicate: {gameObject.name}");
        Destroy(gameObject);
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Manually destroy this persistent object.
    /// Useful for cleanup or resetting game state.
    /// </summary>
    public void DestroyPersistentObject()
    {
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Checks if this object is currently persistent.
    /// </summary>
    public bool IsPersistent()
    {
        return gameObject.scene.name == "DontDestroyOnLoad";
    }
    
    /// <summary>
    /// Gets the persistence ID of this object.
    /// </summary>
    public string GetPersistenceID()
    {
        return persistenceID;
    }
    
    #endregion
    
    #region Static Utility Methods
    
    /// <summary>
    /// Clears all registered persistent instances.
    /// Useful for completely resetting the game state.
    /// </summary>
    public static void ClearAllPersistentInstances()
    {
        existingInstances.Clear();
        Debug.Log("[Persistence] Cleared all persistent instance registrations");
    }
    
    /// <summary>
    /// Returns the number of registered persistent instances.
    /// </summary>
    public static int GetPersistentInstanceCount()
    {
        return existingInstances.Count;
    }
    
    /// <summary>
    /// Checks if a specific persistence ID is already registered.
    /// </summary>
    public static bool IsIDRegistered(string id)
    {
        return existingInstances.Contains(id);
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
    /// Logs all currently registered persistent instances.
    /// </summary>
    [ContextMenu("Log All Persistent Instances")]
    public static void LogAllPersistentInstances()
    {
        Debug.Log("=== PERSISTENT INSTANCES ===");
        Debug.Log($"Total Registered: {existingInstances.Count}");
        
        foreach (string id in existingInstances)
        {
            Debug.Log($"  - {id}");
        }
        
        Debug.Log("===========================");
    }
    
    #endregion
}