using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class GlassSmashEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform smashLocation;      // empty GameObject where glass falls
    [SerializeField] private Transform playerCamera;       // drag Camera here
    [SerializeField] private Transform playerBody;         // drag First Person Player here

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip glassSmashClip;
    [SerializeField] private AudioClip crowdCheerClip;
    [SerializeField] private AudioClip sharpBreathClip;

    [Header("Dialogue")]
    [SerializeField] private string yarnNode = "GlassSmash";

    [Header("NPCs")]
    [SerializeField] private Transform[] allNPCs;          // drag all NPCs here
    [SerializeField] private float npcLookDuration = 3f;   // how long they look at smash
    [SerializeField] private float npcTurnSpeed = 5f;

    [Header("Broken Glass")]
[SerializeField] private GameObject brokenGlassProp;

    private DialogueRunner dialogueRunner;
    private bool hasTriggered = false;

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    /// <summary>
    /// Called by OrderManager after customer 3 is served
    /// </summary>
    public void TriggerGlassSmash()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        StartCoroutine(GlassSmashSequence());
    }

    private IEnumerator GlassSmashSequence()
    {
        // Disable player look controls
        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowCursorPublic();

        var mouseLook = playerCamera.GetComponent<SojaExiles.MouseLook>();
        var playerMovement = playerBody.GetComponent<PlayerMovement>();

        if (mouseLook != null) mouseLook.enabled = false;
        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        // Cut camera to face smash location instantly
        if (smashLocation != null && playerCamera != null)
        {
            Vector3 direction = (smashLocation.position - playerCamera.position).normalized;
            if (direction != Vector3.zero)
            {
                // Rotate player body horizontally
                Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
                playerBody.rotation = Quaternion.LookRotation(flatDirection);

                // Rotate camera vertically
                float verticalAngle = Mathf.Asin(direction.y) * Mathf.Rad2Deg;
                playerCamera.localRotation = Quaternion.Euler(-verticalAngle, 0f, 0f);
            }
        }

        // Play glass smash
        if (audioSource != null && glassSmashClip != null)
            audioSource.PlayOneShot(glassSmashClip);

        yield return new WaitForSeconds(1.5f);

        // Play crowd cheer
        if (audioSource != null && crowdCheerClip != null)
            audioSource.PlayOneShot(crowdCheerClip);

        // All NPCs turn to look at smash location
        StartCoroutine(NPCsLookAtSmash());

        yield return new WaitForSeconds(1.5f);

        // Sharp breath
        if (audioSource != null && sharpBreathClip != null)
            audioSource.PlayOneShot(sharpBreathClip);

        yield return new WaitForSeconds(0.5f);

        // Re-enable controls before dialogue so cursor works
        if (mouseLook != null) mouseLook.enabled = true;
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();

        // Fire Yarn dialogue
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
            if (canvasComponent != null)
            {
                canvasComponent.gameObject.SetActive(true);
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasGroup group = canvasComponent.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
            }

            bool dialogueDone = false;
            dialogueRunner.onDialogueComplete.AddListener(() => dialogueDone = true);
            dialogueRunner.StartDialogue(yarnNode);

            while (!dialogueDone) yield return null;
            dialogueRunner.onDialogueComplete.RemoveListener(() => dialogueDone = true);

            // Show task and activate broken glass prop
            TaskManager.Instance?.ShowTask("Clean up the glass");
            if (brokenGlassProp != null) brokenGlassProp.SetActive(true);
        }
    }

    private IEnumerator NPCsLookAtSmash()
    {
        if (allNPCs == null || smashLocation == null) yield break;

        // Store original rotations
        Quaternion[] originalRotations = new Quaternion[allNPCs.Length];
        for (int i = 0; i < allNPCs.Length; i++)
        {
            if (allNPCs[i] != null)
                originalRotations[i] = allNPCs[i].rotation;
        }

        // Smoothly turn all NPCs to face smash
        float elapsed = 0f;
        float turnDuration = 0.5f;
        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnDuration;

            foreach (Transform npc in allNPCs)
            {
                if (npc == null) continue;
                Vector3 dir = (smashLocation.position - npc.position).normalized;
                dir.y = 0f;
                if (dir != Vector3.zero)
                {
                    Quaternion target = Quaternion.LookRotation(dir);
                    npc.rotation = Quaternion.Slerp(npc.rotation, target, t);
                }
            }
            yield return null;
        }

        // Hold looking at smash
        yield return new WaitForSeconds(npcLookDuration);

        // Smoothly return to original rotations
        elapsed = 0f;
        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnDuration;

            for (int i = 0; i < allNPCs.Length; i++)
            {
                if (allNPCs[i] != null)
                    allNPCs[i].rotation = Quaternion.Slerp(allNPCs[i].rotation, originalRotations[i], t);
            }
            yield return null;
        }
    }
}