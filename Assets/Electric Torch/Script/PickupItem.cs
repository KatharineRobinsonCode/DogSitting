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

        // Show internal dialogue first, then choice
        if (!string.IsNullOrEmpty(inspectDialogueNode))
        {
            bool dialogueDone = false;
            dialogueRunner.onDialogueComplete.AddListener(() => dialogueDone = true);
            dialogueRunner.StartDialogue(inspectDialogueNode);
            StartCoroutine(WaitThenShowChoice(dialogueDone));
        }
        else
        {
            ShowPickupChoice();
        }
    }

    private System.Collections.IEnumerator WaitThenShowChoice(bool dialogueDone)
    {
        while (!dialogueDone)
            yield return null;

        dialogueRunner.onDialogueComplete.RemoveAllListeners();
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