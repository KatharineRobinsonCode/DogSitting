using UnityEngine;

public class BathroomDoor : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt()
    {
        return BathroomLockEvent.Instance?.GetInteractionPrompt() ?? "";
    }

    public void Interact(PlayerInteraction player)
    {
        BathroomLockEvent.Instance?.Interact(player);
    }
}