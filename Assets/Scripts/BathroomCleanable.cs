using UnityEngine;

public class BathroomCleanable : MonoBehaviour, IInteractable
{
    [SerializeField] private string promptText = "Press E to clean up";

    private bool isCleaned = false;
    private bool isActive = false;

    public void Activate()
    {
        isActive = true;
    }

    public string GetInteractionPrompt()
    {
        if (!isCleaned && isActive) return promptText;
        return "";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isCleaned || !isActive) return;

        isCleaned = true;
        gameObject.SetActive(false);

        BathroomLockEvent.Instance?.OnBathroomSpotCleaned();
    }
}