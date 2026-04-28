using UnityEngine;
using SojaExiles;

public class Closet : MonoBehaviour, IInteractable
{
    private ClosetopencloseDoor closetScript;

    private void Awake()
    {
        closetScript = GetComponentInParent<ClosetopencloseDoor>();
        if (closetScript != null)
            closetScript.enabled = false; // hand control to Door.cs
    }

    public string GetInteractionPrompt()
    {
        return closetScript.open ? "Press E to close closet" : "Press E to open closet";
    }

    public void Interact(PlayerInteraction player)
{
    if (closetScript == null) return;

    Debug.Log($"[Closet] Interact called. open = {closetScript.open}");

    if (!closetScript.open)
    {
        closetScript.Closetopenandclose.Play("Opening");
        closetScript.open = true;
    }
    else
    {
        closetScript.Closetopenandclose.Play("Closing");
        closetScript.open = false;
    }
}
}