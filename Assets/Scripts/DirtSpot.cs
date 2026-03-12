using UnityEngine;

public class DirtSpot : MonoBehaviour, IInteractable
{
    private bool isCleaned = false;

    public string GetInteractionPrompt()
    {
        if (!isCleaned)
            return "Press E to clean";
        return "";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isCleaned) return;

        if (!player.IsHoldingBroom)
        {
            FeedbackManager.Instance?.ShowMessage("You need a broom first!", FeedbackManager.MessageType.Error);
            return;
        }

        isCleaned = true;
        gameObject.SetActive(false);

        Debug.Log("[DirtSpot] Cleaned a dirt spot");
        CoffeeShopManager.Instance?.OnDirtSpotCleaned();
    }
}