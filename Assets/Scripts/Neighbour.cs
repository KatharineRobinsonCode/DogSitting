using UnityEngine;
using Yarn.Unity;

public class Neighbour : MonoBehaviour
{
    [SerializeField] private AudioSource stepAudio;
    [SerializeField] private AudioClip stepClip;

    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string dialogueNode = "NeighbourHello";

    private bool hasStepped = false;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasStepped) return;
        if (!other.CompareTag("Player")) return;

        hasStepped = true;
        stepAudio.PlayOneShot(stepClip);

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            dialogueRunner.StartDialogue(dialogueNode);
    }
}