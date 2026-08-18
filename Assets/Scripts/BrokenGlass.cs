using UnityEngine;

public class BrokenGlass : MonoBehaviour, IInteractable
{
    public string GetInteractionPrompt()
    {
        if (!TaskManager.Instance.IsCurrentTask("Clean up the glass")) return "";
        return "Press E to sweep up glass";
    }

    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHoldingBroom)
        {
            FeedbackManager.Instance?.ShowMessage("You need a broom!", FeedbackManager.MessageType.Info);
            return;
        }

        gameObject.SetActive(false);
        TaskManager.Instance?.ShowTask("Serve customers");
    }
}