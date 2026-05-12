using UnityEngine;

public class ExteriorAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource exteriorAmbience;

 private void OnTriggerEnter(Collider other)
{
    if (!other.CompareTag("Player")) return;
    if (HouseSceneState.skipExteriorAudio) return;

    exteriorAmbience.Play();
}
}