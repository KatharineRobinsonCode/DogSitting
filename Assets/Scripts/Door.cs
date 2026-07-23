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
        doorScript = GetComponent<opencloseDoor>();
        if (doorScript == null)
            doorScript = GetComponentInChildren<opencloseDoor>();
        if (doorScript == null)
            doorScript = GetComponentInParent<opencloseDoor>();

        if (doorScript != null)
            doorScript.enabled = false;
    }

    public string GetInteractionPrompt()
    {
        if (isLocked) return "Press E to try door";
        if (doorScript == null) return "Press E to open door";
        return doorScript.open ? "Press E to close door" : "Press E to open door";
    }

    public void Interact(PlayerInteraction player)
    {
        // Handle locked door first — no doorScript needed
        if (isLocked)
        {
            if (doorAudio != null && lockedSound != null)
            {
                doorAudio.clip = lockedSound;
                doorAudio.Play();
                Debug.Log("[Door] Playing locked sound");
            }
            FeedbackManager.Instance?.ShowMessage("It's locked...", FeedbackManager.MessageType.Info);
            return;
        }

        // Opening/closing requires doorScript
        if (doorScript == null)
        {
            Debug.LogWarning("[Door] No opencloseDoor script found — can't open/close");
            return;
        }

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

    public void Unlock() { isLocked = false; }
    public void Lock() { isLocked = true; }
}