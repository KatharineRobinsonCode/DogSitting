using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class WindowNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueNode = "WindowNPCChat";

    [Header("References")]
    [SerializeField] private CarController carController;
    [SerializeField] private Transform woodsWaypoint;
    [SerializeField] private PoliceCall policeCall;

    [Header("Walk to Woods")]
    [SerializeField] private float walkSpeed = 2f;

    private DialogueRunner dialogueRunner;
    private bool isActive = false;
    private bool isWalkingAway = false;

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
        if (!isActive || dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        SetupCanvas();

        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowCursorPublic();

        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
        dialogueRunner.StartDialogue(dialogueNode);
    }

    private void OnDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }

    private void OnCallPolice()
    {
        if (policeCall != null)
            policeCall.Begin(OnPoliceCallComplete);
    }

    private void OnPoliceCallComplete()
    {
        // Resume the after police call dialogue
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            SetupCanvas();

            if (PauseManager.Instance != null)
                PauseManager.Instance.ShowCursorPublic();

            dialogueRunner.onDialogueComplete.AddListener(OnAfterPoliceDialogueComplete);
            dialogueRunner.StartDialogue("AfterPoliceCall");
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
        StartWalkingToWoods();
    }

    public void StartWalkingToWoods()
    {
        isActive = false;
        isWalkingAway = true;
        StartCoroutine(WalkToWoods());
    }

    private IEnumerator WalkToWoods()
    {
        if (woodsWaypoint == null) yield break;

        while (Vector3.Distance(transform.position, woodsWaypoint.position) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                woodsWaypoint.position,
                walkSpeed * Time.deltaTime
            );

            // Face direction of travel
            Vector3 dir = (woodsWaypoint.position - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 5f
                );

            yield return null;
        }

        // Reached woods — enable car and hide NPC
        gameObject.SetActive(false);
        EnableDriving();
    }

    private void EnableDriving()
    {
        if (carController != null)
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
    }
}