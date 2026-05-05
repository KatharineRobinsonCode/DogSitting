using UnityEngine;
using Yarn.Unity;

public class ZoneDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string yarnNodeName;

    [Header("Settings")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        hasTriggered = true;
        dialogueRunner.StartDialogue(yarnNodeName);
    }
}