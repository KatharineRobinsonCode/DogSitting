using UnityEngine;

public class ExteriorAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource exteriorAmbience;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[ExteriorTrigger] Fired! isPlaying: {exteriorAmbience.isPlaying}");
            exteriorAmbience.Play();
        }
    }
}