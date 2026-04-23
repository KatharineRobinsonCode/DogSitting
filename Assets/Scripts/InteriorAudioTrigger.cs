using UnityEngine;

public class InteriorAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource exteriorAmbience;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            exteriorAmbience.Pause();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check which side the player exited to
            Vector3 exitDirection = other.transform.position - transform.position;
            
            // If they exited toward the exterior (forward direction), resume audio
            if (Vector3.Dot(exitDirection.normalized, transform.forward) > 0)
                exteriorAmbience.Play();
            // If they exited toward the interior, do nothing
        }
    }
}