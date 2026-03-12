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
        gameObject.SetActive(false);

        Debug.Log("[Broom] Picked up broom");
        FeedbackManager.Instance?.ShowMessage("Broom picked up!", FeedbackManager.MessageType.Success);
    }
}