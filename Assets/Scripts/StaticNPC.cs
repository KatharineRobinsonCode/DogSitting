using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class StaticNPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private string dialogueNode = "NPCChat";
    [SerializeField] private float interactDistance = 3f;

    [Header("Look At Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float turnSpeed = 7f;

    private DialogueRunner dialogueRunner;
    private bool isFacingPlayer = false;

    private void Start()
    {
        StartCoroutine(LateFindDialogueRunner());
    }

    private IEnumerator LateFindDialogueRunner()
    {
        yield return new WaitForEndOfFrame();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    private void Update()
    {
        if (!isFacingPlayer || player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * turnSpeed
            );
        }
    }

    public string GetInteractionPrompt()
    {
        return "Press E to chat";
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        isFacingPlayer = true;

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
        dialogueRunner.StartDialogue(dialogueNode);
    }

    private void OnDialogueComplete()
    {
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        isFacingPlayer = false;

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }
}