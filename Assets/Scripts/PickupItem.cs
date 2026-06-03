using UnityEngine;
using Yarn.Unity;

public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField] private InventoryItemData itemData;

    [Header("Dialogue")]
    [SerializeField] private string inspectDialogueNode;

    [Header("Pickup Choice UI")]
    [SerializeField] private GameObject pickupChoicePanel;
    [SerializeField] private string pickupPrompt = "Press E to pick up";
    [SerializeField] private string leavePrompt = "Press L to leave";

    private DialogueRunner dialogueRunner;
    private bool isInspecting = false;
    private bool hasBeenPickedUp = false;
    private bool dialogueDone = false;  // ← now a field, not a local variable

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (dialogueRunner != null)
            dialogueRunner.AddCommandHandler("PickUpItem", OnPickUpCommand);

        if (pickupChoicePanel != null)
            pickupChoicePanel.SetActive(false);
    }

    public string GetInteractionPrompt()
    {
        if (hasBeenPickedUp || isInspecting) return "";
        return "Press E to inspect";
    }

    public void Interact(PlayerInteraction player)
    {
        if (hasBeenPickedUp || isInspecting) return;
        if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

        isInspecting = true;

        if (!string.IsNullOrEmpty(inspectDialogueNode))
        {
            dialogueDone = false;  // ← reset before starting
            dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            dialogueRunner.StartDialogue(inspectDialogueNode);
            StartCoroutine(WaitThenShowChoice());
        }
        else
        {
            ShowPickupChoice();
        }
    }

    private void OnDialogueComplete()
    {
        dialogueDone = true;
        dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
    }

    private System.Collections.IEnumerator WaitThenShowChoice()
    {
        while (!dialogueDone)
            yield return null;

        ShowPickupChoice();
    }

    private void ShowPickupChoice()
    {
        if (pickupChoicePanel != null)
            pickupChoicePanel.SetActive(true);

        StartCoroutine(WaitForPickupChoice());
    }

    private System.Collections.IEnumerator WaitForPickupChoice()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUp();
                yield break;
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Leave();
                yield break;
            }
            yield return null;
        }
    }

    private void PickUp()
    {
        if (pickupChoicePanel != null)
            pickupChoicePanel.SetActive(false);

        hasBeenPickedUp = true;

        if (InventoryManager.Instance != null && itemData != null)
            InventoryManager.Instance.AddItem(itemData);

        gameObject.SetActive(false);
        isInspecting = false;
    }

    private void Leave()
    {
        if (pickupChoicePanel != null)
            pickupChoicePanel.SetActive(false);

        isInspecting = false;
        Debug.Log($"[PickupItem] Player left {itemData?.itemName}");
    }

    private void OnPickUpCommand()
    {
        PickUp();
    }
}