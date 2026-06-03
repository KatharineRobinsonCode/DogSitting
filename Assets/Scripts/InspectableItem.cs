using UnityEngine;
using Yarn.Unity;

public class InspectableItem : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Press E to inspect";
    [SerializeField] private string dialogueNode = "DogCollarThought";

    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = true;

    private DialogueRunner dialogueRunner;
    private bool hasBeenInspected = false;

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public string GetInteractionPrompt()
    {
        if (oneTimeOnly && hasBeenInspected) return "";
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning) return "";
        return interactionPrompt;
    }

    public void Interact(PlayerInteraction player)
    {
        if (dialogueRunner == null) return;
        if (dialogueRunner.IsDialogueRunning) return;
        if (oneTimeOnly && hasBeenInspected) return;

        hasBeenInspected = true;
        dialogueRunner.StartDialogue(dialogueNode);
    }
}