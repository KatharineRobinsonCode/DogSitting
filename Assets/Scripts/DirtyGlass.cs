using UnityEngine;

public class DirtyGlass : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt()
    {
        return "Press E to clean up";
    }

    public void Interact(PlayerInteraction player)
    {
        // Could add to a counter here later if you want to track glasses collected
        gameObject.SetActive(false);
        Debug.Log("[DirtyGlass] Object collected");
    }
}