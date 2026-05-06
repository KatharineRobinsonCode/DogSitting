using UnityEngine;
using Yarn.Unity;

public class Neighbour : MonoBehaviour
{
    [SerializeField] private AudioSource stepAudio;
    [SerializeField] private AudioClip stepClip;

    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string dialogueNode = "NeighbourHello";
    [SerializeField] private float dialogueDelay = 3f;

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
        StartCoroutine(ShowDialogueAfterDelay());
    }

    private System.Collections.IEnumerator ShowDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDelay);

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            dialogueRunner.StartDialogue(dialogueNode);
    }
}