using UnityEngine;
using SojaExiles;

public class Door : MonoBehaviour, IInteractable
{
    private opencloseDoor doorScript;

    private void Awake()
    {
        doorScript = GetComponentInParent<opencloseDoor>();
        if (doorScript != null)
            doorScript.enabled = false; // hand control to Door.cs
    }

    public string GetInteractionPrompt()
    {
        return doorScript.open ? "Press E to close door" : "Press E to open door";
    }

    public void Interact(PlayerInteraction player)
{
    if (doorScript == null) return;

    Debug.Log($"[Door] Interact called. open = {doorScript.open}");

    if (!doorScript.open)
    {
        doorScript.openandclose.Play("Opening");
        doorScript.open = true;
    }
    else
    {
        doorScript.openandclose.Play("Closing");
        doorScript.open = false;
    }
}
}