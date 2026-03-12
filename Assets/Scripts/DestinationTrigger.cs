using UnityEngine;

public class DestinationTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            DrivingSceneManager manager = FindFirstObjectByType<DrivingSceneManager>();
            manager?.ReachDestination();
        }
    }
}