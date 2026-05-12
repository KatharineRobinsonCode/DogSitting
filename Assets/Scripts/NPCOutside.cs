using UnityEngine;
using Yarn.Unity;

public class NPCOutside : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string dialogueNode;
    [SerializeField] private bool interactOnce = true;

    private bool hasInteracted = false;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public string GetInteractionPrompt()
    {
        if (interactOnce && hasInteracted) return "";
        return "Press E to Talk";
    }

    public void Interact(PlayerInteraction player)
    {
        if (interactOnce && hasInteracted) return;
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        hasInteracted = true;
        dialogueRunner.StartDialogue(dialogueNode);
    }
}