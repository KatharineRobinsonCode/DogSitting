using UnityEngine;
using Yarn.Unity;

public class NPCOutside : MonoBehaviour, IInteractable
{
    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string dialogueNode;
    [SerializeField] private bool interactOnce = true;

    [Header("Look At Player")]
    [SerializeField] private Transform player;
    [SerializeField] private float turnSpeed = 7f;

    private bool hasInteracted = false;
    private bool isFacingPlayer = false;
    private Quaternion originalRotation;

    private void Start()
    {
        if (dialogueRunner == null)
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
        if (interactOnce && hasInteracted) return "";
        return "Press E to Talk";
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        if (interactOnce && hasInteracted) return;
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        originalRotation = transform.rotation;
        isFacingPlayer = true;
        hasInteracted = true;

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

        StartCoroutine(TurnBackRoutine());
    }

    private System.Collections.IEnumerator TurnBackRoutine()
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = originalRotation;
    }
}