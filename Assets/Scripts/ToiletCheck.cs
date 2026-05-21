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
    [SerializeField] private AudioSource cryingAudio;

    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string toiletDialogueNode = "ToiletKnock";

    private bool hasTriggered = false;

    private void Start()
    {
        StartCoroutine(LateRegisterCommands());
    }

    private IEnumerator LateRegisterCommands()
    {
        yield return new WaitForEndOfFrame();

        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler("OnKnockYes", OnKnockYes);
            dialogueRunner.AddCommandHandler("OnKnockNo", OnKnockNo);
            Debug.Log("[ToiletCheck] Commands registered");
        }
        else
        {
            Debug.LogError("[ToiletCheck] DialogueRunner not found!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ToiletCheck] Trigger entered by: {other.name}");

        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        bool isCorrectTask = TaskManager.Instance.IsCurrentTask("Check on the customer in the toilet");
        Debug.Log($"[ToiletCheck] Is correct task: {isCorrectTask}, current task: {TaskManager.Instance.CurrentTask}");

        if (!isCorrectTask) return;

        hasTriggered = true;
        Debug.Log("[ToiletCheck] Starting dialogue");

        // Start crying audio
        if (cryingAudio != null)
        {
            cryingAudio.loop = true;
            cryingAudio.Play();
        }

        // Set up canvas
        Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
        if (canvasComponent != null)
        {
            canvasComponent.gameObject.SetActive(true);
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasGroup group = canvasComponent.gameObject.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 1f;
        }

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
        // Stop crying when player walks away
        if (cryingAudio != null)
            cryingAudio.Stop();

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();

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
            Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
            if (canvasComponent != null)
            {
                canvasComponent.gameObject.SetActive(true);
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasGroup group = canvasComponent.gameObject.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
            }

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

        // Stop crying after scream sequence
        if (cryingAudio != null)
            cryingAudio.Stop();

        CoffeeShopManager.Instance?.OnToiletTaskComplete();
    }
}