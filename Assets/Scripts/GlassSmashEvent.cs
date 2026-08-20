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

    [Header("Fleeing Figure")]
    [SerializeField] private GameObject fleeingFigure;        // WindowNPC prefab instance — start inactive
    [SerializeField] private Transform figureSpawnPoint;      // where he stands at smash location
    [SerializeField] private Transform doorPosition;          // empty GameObject at the door
    [SerializeField] private Transform doorLookTarget;        // empty GameObject player looks at after
    [SerializeField] private float eyeContactDuration = 2.5f; // pause while looking at each other
    [SerializeField] private float figureVanishDistance = 0.5f; // how close to door before vanishing
    [SerializeField] private float figureWalkSpeed = 4f;

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

        // Make sure figure starts hidden
        if (fleeingFigure != null) fleeingFigure.SetActive(false);
    }

    public void TriggerGlassSmash()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        StartCoroutine(GlassSmashSequence());
    }

    private IEnumerator GlassSmashSequence()
    {
        // Disable player controls
        var mouseLook = playerCamera.GetComponent<SojaExiles.MouseLook>();
        var playerMovement = playerBody.GetComponent<PlayerMovement>();
        if (mouseLook != null) mouseLook.enabled = false;
        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        // Play glass smash
        if (audioSource != null && glassSmashClip != null)
            audioSource.PlayOneShot(glassSmashClip, 2f);

        // Cut camera to face smash location instantly
        LookAt(smashLocation.position);
        StartCoroutine(ZoomIn());

yield return new WaitForSeconds(0.3f);

        // Gasp immediately after smash — before eye contact
if (audioSource != null && sharpBreathClip != null)
    audioSource.PlayOneShot(sharpBreathClip);

yield return new WaitForSeconds(0.5f);

        // Activate figure — eye contact begins
        if (fleeingFigure != null)
        {
            fleeingFigure.transform.position = figureSpawnPoint.position;
            fleeingFigure.SetActive(true);

            // Face the player
            Vector3 dirToPlayer = (playerBody.position - fleeingFigure.transform.position).normalized;
            dirToPlayer.y = 0f;
            if (dirToPlayer != Vector3.zero)
                fleeingFigure.transform.rotation = Quaternion.LookRotation(dirToPlayer);
        }

        // NPCs all turn to look
        StartCoroutine(NPCsLookAtSmash());

        // Eye contact moment — just silence and looking
        yield return new WaitForSeconds(eyeContactDuration);

        // Figure starts walking to the door via NavMesh
        NavMeshAgent agent = fleeingFigure?.GetComponent<NavMeshAgent>();
        Animator figureAnim = fleeingFigure?.GetComponentInChildren<Animator>();

        if (agent != null && doorPosition != null)
        {
            agent.enabled = true;
            agent.speed = figureWalkSpeed;
            agent.SetDestination(doorPosition.position);

            if (figureAnim != null)
                figureAnim.SetBool("isWalking", true);

            // Wait until he's close to the door then vanish
            while (fleeingFigure != null &&
                   Vector3.Distance(fleeingFigure.transform.position, doorPosition.position) > figureVanishDistance)
            {
                yield return null;
            }
        }

        // Vanish at the door
        if (fleeingFigure != null) fleeingFigure.SetActive(false);

        // Cut camera to look at empty door
        if (doorLookTarget != null)
            LookAt(doorLookTarget.position);
            StartCoroutine(ZoomOut());
            
        yield return new WaitForSeconds(0.8f);

        // Sharp breath
        if (audioSource != null && sharpBreathClip != null)
            audioSource.PlayOneShot(sharpBreathClip);

        yield return new WaitForSeconds(0.8f);

        // Restore controls before dialogue
        if (mouseLook != null) mouseLook.enabled = true;
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);
        if (PauseManager.Instance != null) PauseManager.Instance.HideCursorPublic();

        // Fire dialogue
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

        // Show task and activate broken glass
        TaskManager.Instance?.ShowTask("Clean up the glass");
        if (brokenGlassProp != null) brokenGlassProp.SetActive(true);
    }

    private void LookAt(Vector3 targetPosition)
    {
        if (playerCamera == null || playerBody == null) return;

        Vector3 direction = (targetPosition - playerCamera.position).normalized;

        // Rotate player body horizontally
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
        if (flatDirection != Vector3.zero)
            playerBody.rotation = Quaternion.LookRotation(flatDirection);

        // Rotate camera vertically
        float verticalAngle = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
        playerCamera.localRotation = Quaternion.Euler(-verticalAngle, 0f, 0f);
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
    private IEnumerator NPCsLookAtSmash()
    {
        if (allNPCs == null || smashLocation == null) yield break;

        Quaternion[] originalRotations = new Quaternion[allNPCs.Length];
        for (int i = 0; i < allNPCs.Length; i++)
            if (allNPCs[i] != null)
                originalRotations[i] = allNPCs[i].rotation;

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
                    npc.rotation = Quaternion.Slerp(npc.rotation, Quaternion.LookRotation(dir), t);
            }
            yield return null;
        }

        yield return new WaitForSeconds(npcLookDuration);

        elapsed = 0f;
        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnDuration;
            for (int i = 0; i < allNPCs.Length; i++)
                if (allNPCs[i] != null)
                    allNPCs[i].rotation = Quaternion.Slerp(allNPCs[i].rotation, originalRotations[i], t);
            yield return null;
        }
    }
}