using UnityEngine;

public class HouseTaskStarter : MonoBehaviour
{
    private bool carolTextTriggered = false;
    private bool pizzaOrderTriggered = false;

    private void Start()
    {
        if (TaskManager.Instance == null)
        {
            Debug.LogWarning("[HouseTaskStarter] No TaskManager found in scene!");
            return;
        }
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
        if (TaskManager.Instance == null || PhoneManager.Instance == null) return;

        if (!carolTextTriggered && TaskManager.Instance.IsCurrentTask("Text Carol"))
        {
            carolTextTriggered = true;
            PhoneManager.Instance.ReceiveCarolCheckIn();
        }

        if (!pizzaOrderTriggered && TaskManager.Instance.IsCurrentTask("Order Pizza"))
        {
            pizzaOrderTriggered = true;
            PhoneManager.Instance.OpenPizzaOrder();
        }
    }
}