using UnityEngine;

public class TextCarol : MonoBehaviour, IInteractable
{
    private bool hasSent = false;

    public string GetInteractionPrompt()
    {
        if (hasSent) return "";
        if (TaskManager.Instance != null && TaskManager.Instance.IsCurrentTask("Text Carol"))
            return "Press E to text Carol";
        return "";
    }

    public void Interact(PlayerInteraction player)
    {
        if (hasSent) return;
        if (TaskManager.Instance == null || !TaskManager.Instance.IsCurrentTask("Text Carol")) return;

        hasSent = true;
        PhoneManager.Instance.ReceiveCarolCheckIn();
    }
}