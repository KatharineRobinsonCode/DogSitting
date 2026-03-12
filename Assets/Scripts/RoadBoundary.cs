using UnityEngine;

public class RoadBoundary : MonoBehaviour
{
private void OnTriggerEnter(Collider other)
{
    Debug.Log($"[RoadBoundary] Hit by: {other.name} tag: {other.tag}");
    if (other.CompareTag("Car"))
    {
        DrivingSceneManager manager = FindFirstObjectByType<DrivingSceneManager>();
        manager?.TriggerCrashEnding();
    }
}
}