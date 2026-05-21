using UnityEngine;
using Yarn.Unity;
using System.Collections;
using SojaExiles;

public class NeighbourKnock : MonoBehaviour, IInteractable
{
    [Header("Knocking")]
    [SerializeField] private AudioSource knockAudio;
    [SerializeField] private AudioClip knockClip;

    [Header("Door")]
    [SerializeField] private opencloseDoor neighbourDoor;

    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string dialogueNode = "NeighbourSilence";
    [SerializeField] private float dialogueDelay = 2f;

    private bool hasKnocked = false;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public string GetInteractionPrompt()
    {
        if (hasKnocked) return "";
        return "Press E to knock";
    }

    public void Interact(PlayerInteraction player)
    {
        if (hasKnocked) return;
        hasKnocked = true;

        if (knockAudio != null && knockClip != null)
            knockAudio.PlayOneShot(knockClip);

        StartCoroutine(KnockSequence());
    }

    private IEnumerator KnockSequence()
    {
        yield return new WaitForSeconds(dialogueDelay);

        if (neighbourDoor != null && !neighbourDoor.open)
            neighbourDoor.OpenDoor();

        yield return new WaitForSeconds(0.6f);

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            if (PauseManager.Instance != null)
                PauseManager.Instance.ShowCursorPublic();

            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            dialogueRunner.StartDialogue(dialogueNode);
        }
    }

    private void OnDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();

        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        if (neighbourDoor != null && neighbourDoor.open)
            neighbourDoor.CloseDoor();

        yield return new WaitForSeconds(1f);

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            dialogueRunner.StartDialogue("AfterKnock");
    }
}