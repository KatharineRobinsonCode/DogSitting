using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class HouseTaskStarter : MonoBehaviour
{
    private bool carolTextTriggered = false;
    private bool pizzaWaitTriggered = false;
    private bool feedBrinkleyTriggered = false;

    private DialogueRunner dialogueRunner;

 private void Start()
{
    if (TaskManager.Instance == null)
    {
        Debug.LogWarning("[HouseTaskStarter] No TaskManager found in scene!");
        return;
    }

    dialogueRunner = FindFirstObjectByType<DialogueRunner>();

    // Skip task setup if returning from mini game — PizzaBoxTrigger handles task restoration
    if (HouseSceneState.isReturningFromMiniGame) return;

    TaskManager.Instance.SetTaskSequence(
        "Go to Carol's flat on the first floor",
        "Find Brinkley",
        "Feed Brinkley",
        "Text Carol",
        "Order Pizza",
        "Wait for the Pizza"
    );
}

   private void Update()
{
    if (TaskManager.Instance == null) return;
    if (HouseSceneState.isReturningFromMiniGame) return;

    if (!feedBrinkleyTriggered &&
        TaskManager.Instance.IsCurrentTask("Feed Brinkley"))
    {
        feedBrinkleyTriggered = true;
        StartCoroutine(DelayedDialogue("FeedBrinkley", 4f));
    }

    if (!carolTextTriggered && PhoneManager.Instance != null &&
        TaskManager.Instance.IsCurrentTask("Text Carol"))
    {
        carolTextTriggered = true;
        PhoneManager.Instance.ReceiveCarolCheckIn();
    }

    if (!pizzaWaitTriggered &&
        TaskManager.Instance.IsCurrentTask("Wait for the Pizza"))
    {
        pizzaWaitTriggered = true;

        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            dialogueRunner.StartDialogue("PizzaWait");
    }
}
}