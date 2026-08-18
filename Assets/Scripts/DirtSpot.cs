using UnityEngine;

public class DirtSpot : MonoBehaviour, IInteractable
{
    private bool isCleaned = false;
    private bool isActive = false; // Only cleanable after shift ends

    public void Activate()
    {
        isActive = true;
    }

    public string GetInteractionPrompt()
    {
        if (!isCleaned && isActive)
            return "Press E to clean";
        return "";
    }
public void Interact(PlayerInteraction player)
{
    if (isCleaned) return;
    if (!isActive) return;
    if (!player.IsHoldingBroom)
    {
        FeedbackManager.Instance?.ShowMessage("You need a broom first!", FeedbackManager.MessageType.Error);
        return;
    }

    isCleaned = true;
    gameObject.SetActive(false);
    Debug.Log($"[DirtSpot] Cleaned — CoffeeShopManager null: {CoffeeShopManager.Instance == null}");
    CoffeeShopManager.Instance?.OnDirtSpotCleaned();
}
}