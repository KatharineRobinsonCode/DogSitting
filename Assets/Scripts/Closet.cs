using UnityEngine;
using SojaExiles;

public class Closet : MonoBehaviour, IInteractable
{
    private ClosetopencloseDoor closetScript;

    private void Awake()
    {
        closetScript = GetComponent<ClosetopencloseDoor>(); // same object, not parent
        if (closetScript != null)
            closetScript.enabled = false;
    }

    public string GetInteractionPrompt()
    {
        if (closetScript == null) return "Press E to open closet";
        return closetScript.open ? "Press E to close closet" : "Press E to open closet";
    }

    public void Interact(PlayerInteraction player)
    {
        if (closetScript == null) return;

        if (!closetScript.open)
        {
            closetScript.Closetopenandclose.Play("ClosetOpening"); // fixed name
            closetScript.open = true;
        }
        else
        {
            closetScript.Closetopenandclose.Play("ClosetClosing"); // fixed name
            closetScript.open = false;
        }
    }
}