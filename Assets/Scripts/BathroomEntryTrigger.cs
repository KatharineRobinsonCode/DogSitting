using UnityEngine;

public class BathroomEntryTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        BathroomLockEvent.Instance?.OnPlayerEnteredBathroom();
    }
}