using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class WindowNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueNode = "WindowNPCChat";
    [SerializeField] private string afterPoliceCallNode = "AfterPoliceCall";

    [Header("References")]
    [SerializeField] private CarController carController;
    [SerializeField] private Transform woodsWaypoint;
    [SerializeField] private PoliceCall policeCall;

    [Header("Walk to Woods")]
    [SerializeField] private float walkSpeed = 2f;

    private DialogueRunner dialogueRunner;
    private bool isActive = false;
    private bool isWalkingAway = false;
    private bool policeCallComplete = false;

    private void Start()
    {
        StartCoroutine(LateFindDialogueRunner());
    }

    private IEnumerator LateFindDialogueRunner()
    {
        yield return new WaitForEndOfFrame();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler("CallPolice", OnCallPolice);
            dialogueRunner.AddCommandHandler("DriveOff", OnDriveOff);
            dialogueRunner.AddCommandHandler("NpcWalksAway", StartWalkingToWoods);
        }
    }

    public void Activate()
    {
        isActive = true;
        Debug.Log("[WindowNPC] Activated — press E to interact");
    }

    public string GetInteractionPrompt()
    {
        if (!isActive || isWalkingAway) return "";
        return "Press E to talk";
    }

   public void Interact(PlayerInteraction player)
{
    Debug.Log($"[WindowNPC] Interact called — isActive: {isActive}, isRunning: {dialogueRunner?.IsDialogueRunning}");
    if (!isActive || dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

    Debug.Log("[WindowNPC] Setting up canvas");
    SetupCanvas();
    
    Debug.Log($"[WindowNPC] Starting dialogue node: {(policeCallComplete ? afterPoliceCallNode : dialogueNode)}");
    dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
    dialogueRunner.StartDialogue(policeCallComplete ? afterPoliceCallNode : dialogueNode);
    Debug.Log("[WindowNPC] StartDialogue called");
}

    private void OnDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }

    private void OnCallPolice()
    {
        Debug.Log("[WindowNPC] OnCallPolice fired!");
        StartCoroutine(WaitThenCall());
    }

    private IEnumerator WaitThenCall()
    {
        while (dialogueRunner.IsDialogueRunning)
            yield return null;

        if (policeCall != null)
            policeCall.Begin(OnPoliceCallComplete);
        else
            Debug.LogError("[WindowNPC] PoliceCall is null!");
    }

    private void OnPoliceCallComplete()
    {
        policeCallComplete = true;

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            SetupCanvas();
            dialogueRunner.onDialogueComplete.AddListener(OnAfterPoliceDialogueComplete);
            dialogueRunner.StartDialogue(afterPoliceCallNode);
        }
    }

    private void OnAfterPoliceDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnAfterPoliceDialogueComplete);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();

        StartWalkingToWoods();
    }

   private void OnDriveOff()
{
    Debug.Log("[WindowNPC] OnDriveOff fired!");
    StartCoroutine(WaitThenDriveOff());
}

private IEnumerator WaitThenDriveOff()
{
    while (dialogueRunner != null && dialogueRunner.IsDialogueRunning)
        yield return null;

    StartWalkingToWoods();
}

    public void StartWalkingToWoods()
    {
        Debug.Log("[WindowNPC] StartWalkingToWoods called!");
        isActive = false;
        isWalkingAway = true;
        StartCoroutine(WalkToWoods());
    }

private IEnumerator WalkToWoods()
{
    Debug.Log("[WindowNPC] WalkToWoods started");
    
    if (woodsWaypoint == null)
    {
        Debug.LogError("[WindowNPC] woodsWaypoint is NULL — EnableDriving will never be called!");
        yield break;
    }

    Debug.Log("[WindowNPC] Walking to waypoint...");
    
    Animator anim = GetComponentInChildren<Animator>();
    if (anim != null)
        anim.SetFloat("Speed", walkSpeed);

    while (Vector3.Distance(transform.position, woodsWaypoint.position) > 0.5f)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            woodsWaypoint.position,
            walkSpeed * Time.deltaTime
        );

        Vector3 dir = (woodsWaypoint.position - transform.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 5f
            );

        yield return null;
    }

    Debug.Log("[WindowNPC] Reached waypoint — calling EnableDriving");
    
    if (anim != null)
        anim.SetFloat("Speed", 0f);

    gameObject.SetActive(false);
    EnableDriving();
}
    private void EnableDriving()
    {
         carController.ResumeCar(); 
            carController.EnableControls();

        if (TaskManager.Instance != null)
            TaskManager.Instance.ShowTask("Drive to the house");

        FollowCar followCar = FindFirstObjectByType<FollowCar>();
        if (followCar != null)
            followCar.StartFollowing();
    }

private void SetupCanvas()
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
}
}