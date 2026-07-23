using UnityEngine;

public class Broom : MonoBehaviour, IInteractable
{
    private bool isPickedUp = false;

    public string GetInteractionPrompt()
    {
        if (!isPickedUp)
            return "Press E to pick up broom";
        return "";
    }

 public void Interact(PlayerInteraction player)
{
    if (isPickedUp) return;

    isPickedUp = true;
    player.SetHoldingBroom(true);
    player.PickUpItem(gameObject);  // ← attach to hold point instead of hiding

    Debug.Log("[Broom] Picked up broom");
    FeedbackManager.Instance?.ShowMessage("Broom picked up!", FeedbackManager.MessageType.Success);
}
public void Drop(PlayerInteraction player)
{
    isPickedUp = false;
    player.SetHoldingBroom(false);
    player.DropHeldItem();

    // Re-enable gravity so it falls to the floor
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    Debug.Log("[Broom] Dropped broom");
}
}