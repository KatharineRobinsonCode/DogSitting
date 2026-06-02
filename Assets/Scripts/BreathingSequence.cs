using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System.Collections;

public class BreathingSequence : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image vignetteImage;
    [SerializeField] private float breatheCycleTime = 1.5f;
    [SerializeField] private int breatheCount = 4;
    [SerializeField] private float sleepFadeTime = 2f;
    [SerializeField] private float sleepDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource knockAudio;
    [SerializeField] private AudioClip knockClip;
    [SerializeField] private AudioSource heartbeatAudio;

public void SetHeartbeat(AudioSource audio)
{
    heartbeatAudio = audio;
}

    [Header("Dialogue")]
    [SerializeField] private string breatheDialogueNode = "BreathingThought";

    [Header("References")]
    [SerializeField] private WindowNPC windowNPC;

    private Transform npcTransform;
private Vector3 npcTargetPosition;
private Transform carTransform;

    private DialogueRunner dialogueRunner;
    private bool waitingForE = false;

    private void Start()
    {
        if (vignetteImage != null)
            vignetteImage.color = new Color(0, 0, 0, 0);

        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public void Begin()
    {
        StartCoroutine(BreathingCoroutine());
    }

    private IEnumerator BreathingCoroutine()
    {
        // Show dialogue
        if (dialogueRunner != null)
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

            bool dialogueDone = false;
            dialogueRunner.onDialogueComplete.AddListener(() => dialogueDone = true);
            dialogueRunner.StartDialogue(breatheDialogueNode);

            while (!dialogueDone) yield return null;

            dialogueRunner.onDialogueComplete.RemoveListener(() => dialogueDone = true);

            if (PauseManager.Instance != null)
                PauseManager.Instance.HideCursorPublic();
        }

        // Breathing vignette pulses
        for (int i = 0; i < breatheCount; i++)
        {
            // Breathe in — fade in
            yield return StartCoroutine(FadeVignette(0f, 0.6f, breatheCycleTime * 0.5f));
            // Breathe out — fade out
            yield return StartCoroutine(FadeVignette(0.6f, 0.1f, breatheCycleTime * 0.5f));
        }

        // Fall asleep — fade to full black
        yield return StartCoroutine(FadeVignette(0.1f, 1f, sleepFadeTime));
if (heartbeatAudio != null) heartbeatAudio.Stop();
        yield return new WaitForSeconds(sleepDuration);
// Teleport NPC while screen is fully black — player can't see it happen
if (npcTransform != null)
{
    npcTransform.position = npcTargetPosition;
    if (carTransform != null)
        npcTransform.LookAt(carTransform);
    Debug.Log("[BreathingSequence] NPC teleported to window");
}

        // Knock sound to wake up
        if (knockAudio != null && knockClip != null)
            knockAudio.PlayOneShot(knockClip);

        yield return new WaitForSeconds(0.5f);

        // Fade back in — wake up
        yield return StartCoroutine(FadeVignette(1f, 0f, sleepFadeTime));

        // Activate window NPC interaction
        if (windowNPC != null)
            windowNPC.Activate();
    }

    private IEnumerator FadeVignette(float from, float to, float duration)
    {
        if (vignetteImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(from, to, t);
            vignetteImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        vignetteImage.color = new Color(0, 0, 0, to);
    }
    public void SetNPCTarget(Transform npc, Vector3 targetPos, Transform car)
{
    npcTransform = npc;
    npcTargetPosition = targetPos;
    carTransform = car;
}
}