using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with by the player.
/// Implement this interface on any GameObject that should respond to player interaction.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Returns the text to display when the player looks at this object.
    /// </summary>
    /// <returns>Interaction prompt text (e.g., "Press E to pick up")</returns>
    string GetInteractionPrompt();
    
    /// <summary>
    /// Called when the player interacts with this object.
    /// </summary>
    /// <param name="player">Reference to the player performing the interaction</param>
    void Interact(PlayerInteraction player);
    
    /// <summary>
    /// Optional: Returns whether this object can currently be interacted with.
    /// Implement this to conditionally enable/disable interactions.
    /// </summary>
    /// <returns>True if interaction is allowed, false otherwise</returns>
    bool CanInteract() => true; // Default implementation allows interaction
    
    /// <summary>
    /// Optional: Returns the interaction distance override for this specific object.
    /// Return null to use the default interaction distance.
    /// </summary>
    /// <returns>Custom interaction distance, or null for default</returns>
    float? GetInteractionDistance() => null; // Default uses player's interaction distance
    
    /// <summary>
    /// Optional: Called when the player starts looking at this object.
    /// Useful for highlighting or preview effects.
    /// </summary>
    void OnInteractionFocus() { }
    
    /// <summary>
    /// Optional: Called when the player stops looking at this object.
    /// Useful for removing highlights or preview effects.
    /// </summary>
    void OnInteractionUnfocus() { }
}

/// <summary>
/// Example implementation showing how to use the IInteractable interface.
/// </summary>
/*
public class ExampleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Press E to interact";
    [SerializeField] private bool isInteractable = true;
    [SerializeField] private float customDistance = 5f;
    
    public string GetInteractionPrompt()
    {
        return promptText;
    }
    
    public void Interact(PlayerInteraction player)
    {
        Debug.Log("Player interacted with " + gameObject.name);
    }
    
    public bool CanInteract()
    {
        return isInteractable;
    }
    
    public float? GetInteractionDistance()
    {
        return customDistance;
    }
    
    public void OnInteractionFocus()
    {
        // Add highlight effect
    }
    
    public void OnInteractionUnfocus()
    {
        // Remove highlight effect
    }
}
*/