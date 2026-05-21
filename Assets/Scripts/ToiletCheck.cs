using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class ToiletCheck : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource knockAudio;
    [SerializeField] private AudioClip knockClip;
    [SerializeField] private AudioSource screamAudio;
    [SerializeField] private AudioClip screamClip;

    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string toiletDialogueNode = "ToiletKnock";

    private bool hasTriggered = false;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
            dialogueRunner.AddCommandHandler("OnKnockYes", OnKnockYes);
dialogueRunner.AddCommandHandler("OnKnockNo", OnKnockNo);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        if (!TaskManager.Instance.IsCurrentTask("Check on the customer in the toilet")) return;

        hasTriggered = true;

        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowCursorPublic();

        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        dialogueRunner.StartDialogue(toiletDialogueNode);
    }

    private void OnDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }

    public void OnKnockYes()
    {
        StartCoroutine(KnockSequence());
    }

    public void OnKnockNo()
    {
        CoffeeShopManager.Instance?.OnToiletTaskComplete();
    }

    private IEnumerator KnockSequence()
    {
        if (knockAudio != null && knockClip != null)
            knockAudio.PlayOneShot(knockClip);

        yield return new WaitForSeconds(2f);

        if (screamAudio != null && screamClip != null)
            screamAudio.PlayOneShot(screamClip);

        yield return new WaitForSeconds(0.5f);

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            if (PauseManager.Instance != null)
                PauseManager.Instance.ShowCursorPublic();

            dialogueRunner.onDialogueComplete.AddListener(OnAfterScreamDialogueComplete);
            dialogueRunner.StartDialogue("ToiletAfterScream");
        }
    }

    private void OnAfterScreamDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnAfterScreamDialogueComplete);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();

        CoffeeShopManager.Instance?.OnToiletTaskComplete();
    }
}