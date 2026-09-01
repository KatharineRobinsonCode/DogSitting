using UnityEngine;
using Yarn.Unity;
using System.Collections;
using UnityEngine.AI;

public class GlassSmashEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform smashLocation;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform playerBody;

    [SerializeField] private Transform doorPosition;          // empty GameObject at the door
    [SerializeField] private Transform doorLookTarget;        // empty GameObject player looks at after

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip glassSmashClip;
    [SerializeField] private AudioClip sharpBreathClip;

    [Header("Dialogue")]
    [SerializeField] private string yarnNode = "GlassSmash";

    [Header("NPCs")]
    [SerializeField] private Transform[] allNPCs;
    [SerializeField] private float npcLookDuration = 3f;
    [SerializeField] private float npcTurnSpeed = 5f;

    [Header("Broken Glass")]
    [SerializeField] private GameObject brokenGlassProp;

[Header("Camera Effects")]
[SerializeField] private Camera playerCam;
[SerializeField] private float normalFOV = 60f;
[SerializeField] private float zoomFOV = 45f;
[SerializeField] private float zoomSpeed = 3f;
    private DialogueRunner dialogueRunner;
    private bool hasTriggered = false;

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public void TriggerGlassSmash()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        StartCoroutine(GlassSmashSequence());
    }
    private IEnumerator ZoomIn()
{
    float elapsed = 0f;
    float duration = 0.5f;
    float startFOV = playerCam.fieldOfView;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        playerCam.fieldOfView = Mathf.Lerp(startFOV, zoomFOV, elapsed / duration);
        yield return null;
    }
}

private IEnumerator ZoomOut()
{
    float elapsed = 0f;
    float duration = 0.5f;
    float startFOV = playerCam.fieldOfView;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        playerCam.fieldOfView = Mathf.Lerp(startFOV, normalFOV, elapsed / duration);
        yield return null;
    }
}
private void LookAt(Vector3 targetPosition)
{
    if (playerCamera == null || playerBody == null) return;

    Vector3 direction = (targetPosition - playerCamera.position).normalized;

    Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
    if (flatDirection != Vector3.zero)
        playerBody.rotation = Quaternion.LookRotation(flatDirection);

    float verticalAngle = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
    playerCamera.localRotation = Quaternion.Euler(-verticalAngle, 0f, 0f);
}
private IEnumerator GlassSmashSequence()
{
    var mouseLook = playerCamera.GetComponent<SojaExiles.MouseLook>();
    var playerMovement = playerBody.GetComponent<PlayerMovement>();
    if (mouseLook != null) mouseLook.enabled = false;
    if (playerMovement != null) playerMovement.SetMovementEnabled(false);

    if (audioSource != null && glassSmashClip != null)
        audioSource.PlayOneShot(glassSmashClip, 2f);
    if (audioSource != null && sharpBreathClip != null)
        audioSource.PlayOneShot(sharpBreathClip);

    LookAt(smashLocation.position);
    StartCoroutine(ZoomIn());

    yield return new WaitForSeconds(2f);

    if (doorLookTarget != null)
        LookAt(doorLookTarget.position);
    StartCoroutine(ZoomOut());

    yield return new WaitForSeconds(0.8f);

    if (mouseLook != null) mouseLook.enabled = true;
    if (playerMovement != null) playerMovement.SetMovementEnabled(true);
    if (PauseManager.Instance != null) PauseManager.Instance.HideCursorPublic();

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
    }

    TaskManager.Instance?.ShowTask("Clean up the glass");
    if (brokenGlassProp != null) brokenGlassProp.SetActive(true);
}
}