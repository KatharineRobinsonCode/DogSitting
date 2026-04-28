using UnityEngine;
using SojaExiles;

public class Fridge : MonoBehaviour, IInteractable
{
    private opencloseDoor fridgeScript;

    private void Awake()
    {
        fridgeScript = GetComponentInParent<opencloseDoor>();
        if (fridgeScript != null)
            fridgeScript.enabled = false; // hand control to Fridge.cs
    }

    public string GetInteractionPrompt()
    {
        return fridgeScript.open ? "Press E to close fridge" : "Press E to open fridge";
    }

    public void Interact(PlayerInteraction player)
{
    if (doorScript == null) return;

    Debug.Log($"[Fridge] Interact called. open = {fridgeScript.open}");

    if (!fridgeScript.open)
    {
        fridgeScript.openandclose.Play("Opening");
        fridgeScript.open = true;
    }
    else
    {
        fridgeScript.openandclose.Play("Closing");
        fridgeScript.open = false;
    }
}
}