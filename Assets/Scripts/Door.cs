using UnityEngine;
using SojaExiles;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Lock Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private AudioSource doorAudio;
    [SerializeField] private AudioClip lockedSound;

    private opencloseDoor doorScript;

    private void Awake()
    {
        doorScript = GetComponentInChildren<opencloseDoor>();
        if (doorScript != null)
            doorScript.enabled = false;
    }

   public string GetInteractionPrompt()
{
    if (isLocked) return "Press E to try door";
    return doorScript.open ? "Press E to close door" : "Press E to open door";
}

    public void Interact(PlayerInteraction player)
    {
        if (doorScript == null) return;

      if (isLocked)
{
    if (doorAudio != null)
        doorAudio.Play();
    Debug.Log("[Door] Door is locked");
    return;
}

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

    public void Unlock()
    {
        isLocked = false;
        Debug.Log("[Door] Door unlocked");
    }

    public void Lock()
    {
        isLocked = true;
        Debug.Log("[Door] Door locked");
    }
}