using UnityEngine;

public class HouseTaskStarter : MonoBehaviour
{
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
            "Make dinner"
        );
    }
}