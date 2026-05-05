using UnityEngine;

public class HouseTaskStarter : MonoBehaviour
{
    private bool carolTextTriggered = false;

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
            "Make dinner"
        );
    }

    private void Update()
    {
        if (carolTextTriggered) return;
        if (TaskManager.Instance == null || PhoneManager.Instance == null) return;

        if (TaskManager.Instance.IsCurrentTask("Text Carol"))
        {
            carolTextTriggered = true;
            PhoneManager.Instance.ReceiveCarolCheckIn();
        }
    }
}