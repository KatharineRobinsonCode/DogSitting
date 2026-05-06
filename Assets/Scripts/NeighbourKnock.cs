using UnityEngine;
using Yarn.Unity;

public class NeighbourKnock : MonoBehaviour, IInteractable
{
    [Header("Knocking")]
    [SerializeField] private AudioSource knockAudio;
    [SerializeField] private AudioClip knockClip;

     [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string dialogueNode = "NeighbourSilence";
    [SerializeField] private float dialogueDelay = 2f;

    public string GetInteractionPrompt()
    {
        return "Press E to knock";
    }

    public void Interact(PlayerInteraction player)
    {
        if (knockAudio != null && knockClip != null)
            knockAudio.PlayOneShot(knockClip);
                    StartCoroutine(ShowDialogueAfterDelay());

    }

      private System.Collections.IEnumerator ShowDialogueAfterDelay()
    {
        yield return new WaitForSeconds(dialogueDelay);

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            dialogueRunner.StartDialogue(dialogueNode);
    }
}