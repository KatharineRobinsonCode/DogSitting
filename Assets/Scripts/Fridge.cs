using UnityEngine;
using SojaExiles;

public class Fridge : MonoBehaviour, IInteractable
{
    private opencloseDoor leftDoor;
    private opencloseDoor1 rightDoor;

    private void Awake()
    {
        leftDoor = GetComponentInChildren<opencloseDoor>();
        rightDoor = GetComponentInChildren<opencloseDoor1>();

        if (leftDoor != null) leftDoor.enabled = false;
        if (rightDoor != null) rightDoor.enabled = false;
    }

    public string GetInteractionPrompt()
    {
        bool isOpen = (leftDoor != null && leftDoor.open) || (rightDoor != null && rightDoor.open);
        return isOpen ? "Press E to close fridge" : "Press E to open fridge";
    }

    public void Interact(PlayerInteraction player)
    {
        bool isOpen = (leftDoor != null && leftDoor.open) || (rightDoor != null && rightDoor.open);

        if (!isOpen)
        {
            if (leftDoor != null) { leftDoor.openandclose.Play("Opening"); leftDoor.open = true; }
            if (rightDoor != null) { rightDoor.openandclose.Play("Opening"); rightDoor.open = true; }
        }
        else
        {
            if (leftDoor != null) { leftDoor.openandclose.Play("Closing"); leftDoor.open = false; }
            if (rightDoor != null) { rightDoor.openandclose.Play("Closing"); rightDoor.open = false; }
        }
    }
}