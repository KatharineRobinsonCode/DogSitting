using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class BathroomLockEvent : MonoBehaviour, IInteractable
{
    public static BathroomLockEvent Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform playerBody;
    [SerializeField] private GameObject doorClosed;     // closed door model — start inactive
    [SerializeField] private GameObject doorOpen;       // open door model — start active
    [SerializeField] private GameObject terryNPC;       // Terry's NPC — already in scene
    [SerializeField] private Transform terryOfficeTarget; // where Terry walks back to
    [SerializeField] private GameObject[] npcsToHide;  // all seated NPCs to hide

    [Header("Bathroom Dirt Spots")]
    [SerializeField] private DirtSpot[] bathroomDirtSpots;
    [SerializeField] private int totalBathroomSpots = 3;

   [Header("Audio")]
[SerializeField] private AudioSource audioSource;        // creepy ambience
[SerializeField] private AudioSource footstepsSource;    // separate source for footsteps
[SerializeField] private AudioClip doorSlamClip;
[SerializeField] private AudioClip doorLockClip;
[SerializeField] private AudioClip creepyAmbienceClip;
[SerializeField] private AudioClip footstepsClip;
    [SerializeField] private AudioClip doorBangClip;
    [SerializeField] private AudioClip terryOpenDoorClip;

[Header("Door")]
[SerializeField] private Transform doorPosition;  // empty GameObject at bathroom door threshold
    [Header("Dialogue")]
    [SerializeField] private string lockedDialogueNode = "LockedInBathroom";
    [SerializeField] private string terryArrivalNoBangNode = "TerryArrivalNoBang";
    [SerializeField] private string terryArrivalBangNode = "TerryArrivalBang";
    [SerializeField] private string terryArrivalManyBangsNode = "TerryArrivalManyBangs";
    [SerializeField] private string terryGoodbyeNode = "TerryGoodbye";

    [Header("Timing")]
    [SerializeField] private float baseWaitTime = 30f;
    [SerializeField] private float timeReductionPerBang = 5f;

    [Header("Terry Movement")]
    [SerializeField] private float terryWalkSpeed = 2f;

    [Header("Pub Audio to Stop")]
    [SerializeField] private AudioSource pubMusicSource;
    [SerializeField] private AudioSource pubChatterSource;

    private DialogueRunner dialogueRunner;
    private bool isLocked = false;
    private bool terryHasArrived = false;
    private int bathroomSpotsRemaining;
    private int bathroomSpotsCleaned = 0;
    private int bangCount = 0;
    private float waitTimeRemaining;
    private bool waitingForTerry = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        bathroomSpotsRemaining = totalBathroomSpots;
    }

    // Called when player enters the bathroom trigger
    public void OnPlayerEnteredBathroom()
    {
        if (!TaskManager.Instance.IsCurrentTask("Clean bathroom")) return;

        // Activate bathroom dirt spots
        foreach (DirtSpot spot in bathroomDirtSpots)
            if (spot != null) spot.Activate();

        TaskManager.Instance?.ShowTask($"Clean bathroom (0/{totalBathroomSpots})");
    }

    // Called by each bathroom DirtSpot when cleaned
    public void OnBathroomSpotCleaned()
    {
        bathroomSpotsRemaining--;
        bathroomSpotsCleaned++;

        if (bathroomSpotsRemaining <= 0)
        {
            // All done — bathroom complete
            CoffeeShopManager.Instance?.OnBathroomCleaningComplete();
            return;
        }

        TaskManager.Instance?.ShowTask($"Clean bathroom ({bathroomSpotsCleaned}/{totalBathroomSpots})");

        // After second spot — trigger the lock sequence
        if (bathroomSpotsCleaned == 2)
            StartCoroutine(LockSequence());
    }

    private IEnumerator LockSequence()
    {
        if (pubMusicSource != null) pubMusicSource.Stop();
if (pubChatterSource != null) pubChatterSource.Stop();

        isLocked = true;
        waitTimeRemaining = baseWaitTime;

        yield return new WaitForSeconds(1f);

        // Door slam
        if (audioSource != null && doorSlamClip != null)
            audioSource.PlayOneShot(doorSlamClip);

        // Swap door models
        if (doorOpen != null) doorOpen.SetActive(false);
        if (doorClosed != null) doorClosed.SetActive(true);

        yield return new WaitForSeconds(0.8f);

        // Lock click
        if (audioSource != null && doorLockClip != null)
            audioSource.PlayOneShot(doorLockClip);

        yield return new WaitForSeconds(0.5f);

        // Locked dialogue
        yield return StartCoroutine(PlayDialogue(lockedDialogueNode));

      // Task: text Terry
TaskManager.Instance?.ShowTask("Text Terry");
PhoneManager.Instance?.SendTerryLockedText(onSent: () => StartCoroutine(WaitForTerry()));
    }

    private IEnumerator WaitForTerry()
{
    TaskManager.Instance?.ShowTask("Wait for Terry...");
    waitingForTerry = true;

    // Start creepy ambience
    if (audioSource != null && creepyAmbienceClip != null)
    {
        audioSource.clip = creepyAmbienceClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Start footsteps quietly — escalate as time runs out
    if (footstepsSource != null && footstepsClip != null)
    {
        footstepsSource.clip = footstepsClip;
        footstepsSource.loop = true;
        footstepsSource.volume = 0f;
        footstepsSource.Play();
    }

    // Countdown — reduced by bangs
    while (waitTimeRemaining > 0f)
    {
        waitTimeRemaining -= Time.deltaTime;

        // Escalate footstep volume as timer counts down
        if (footstepsSource != null)
        {
            float progress = 1f - (waitTimeRemaining / baseWaitTime);
            footstepsSource.volume = Mathf.Lerp(0f, 1f, progress);
        }

        yield return null;
    }

    waitingForTerry = false;
    audioSource.Stop();
    if (footstepsSource != null) footstepsSource.Stop();

    yield return StartCoroutine(TerryArrivesSequence());
}

    // IInteractable — door banging
    public string GetInteractionPrompt()
    {
        if (!isLocked || terryHasArrived) return "";
        return "Press E to bang on door";
    }

    public void Interact(PlayerInteraction player)
    {
        if (!isLocked || !waitingForTerry || terryHasArrived) return;

        bangCount++;

        if (audioSource != null && doorBangClip != null)
            audioSource.PlayOneShot(doorBangClip);

        // Reduce wait time
        waitTimeRemaining -= timeReductionPerBang;
        if (waitTimeRemaining < 0) waitTimeRemaining = 0;

        Debug.Log($"[BathroomLock] Bang #{bangCount} — time remaining: {waitTimeRemaining}");
    }

    private IEnumerator TerryArrivesSequence()
    {
        // Make sure Terry is visible
if (terryNPC != null) terryNPC.SetActive(true);

        terryHasArrived = true;

        Debug.Log($"[BathroomLock] Terry position: {terryNPC.transform.position} — door position: {doorPosition.position}");

        // Loud door sound
        if (audioSource != null && terryOpenDoorClip != null)
            audioSource.PlayOneShot(terryOpenDoorClip);

        // Swap door back open
        if (doorClosed != null) doorClosed.SetActive(false);
        if (doorOpen != null) doorOpen.SetActive(true);

        // Hide all other NPCs silently
        foreach (GameObject npc in npcsToHide)
            if (npc != null) npc.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // Pick dialogue based on bang count
        string nodeToPlay = bangCount == 0 ? terryArrivalNoBangNode
                          : bangCount < 3  ? terryArrivalBangNode
                          : terryArrivalManyBangsNode;

        yield return StartCoroutine(PlayDialogue(nodeToPlay));

        // Terry goodbye dialogue
        yield return StartCoroutine(PlayDialogue(terryGoodbyeNode));

        // Terry walks back to office
        StartCoroutine(TerryWalksToOffice());

        // Deep breath Yarn node
        yield return StartCoroutine(PlayDialogue("DeepBreath"));

        // Bathroom cleaning is done — start pub sweep
        isLocked = false;
        CoffeeShopManager.Instance?.OnBathroomCleaningComplete();
    }

 private IEnumerator TerryWalksToOffice()
{
    if (terryNPC == null || terryOfficeTarget == null) yield break;

    UnityEngine.AI.NavMeshAgent agent = terryNPC.GetComponent<UnityEngine.AI.NavMeshAgent>();
    Animator anim = terryNPC.GetComponentInChildren<Animator>();

    if (agent != null)
    {
        agent.speed = terryWalkSpeed;
        agent.SetDestination(terryOfficeTarget.position);
        if (anim != null) anim.SetFloat("Speed", 1f);

        while (Vector3.Distance(terryNPC.transform.position, terryOfficeTarget.position) > 0.5f)
            yield return null;

        if (anim != null) anim.SetFloat("Speed", 0f);
    }

    // Don't deactivate — Terry stays in his chair
    // Later: trigger tip/payment system based on orders served correctly
}

    private IEnumerator PlayDialogue(string nodeName)
    {
        if (dialogueRunner == null || string.IsNullOrEmpty(nodeName)) yield break;

        Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
        if (canvasComponent != null)
        {
            canvasComponent.gameObject.SetActive(true);
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasGroup group = canvasComponent.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = 1f;
        }

        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowCursorPublic();

        bool done = false;
        dialogueRunner.onDialogueComplete.AddListener(() => done = true);
        dialogueRunner.StartDialogue(nodeName);
        while (!done) yield return null;
        dialogueRunner.onDialogueComplete.RemoveListener(() => done = true);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }
}