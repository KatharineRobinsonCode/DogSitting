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
}