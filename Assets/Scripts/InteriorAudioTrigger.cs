using UnityEngine;

public class InteriorAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource exteriorAmbience;

    private void Awake()
    {
        if (HouseSceneState.isReturningFromMiniGame && exteriorAmbience != null)
        {
            exteriorAmbience.Stop();
            exteriorAmbience.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (exteriorAmbience != null && exteriorAmbience.isPlaying)
            exteriorAmbience.Pause();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (HouseSceneState.isReturningFromMiniGame) return;

        Vector3 exitDirection = other.transform.position - transform.position;
        if (Vector3.Dot(exitDirection.normalized, transform.forward) > 0)
        {
            if (exteriorAmbience != null)
            {
                exteriorAmbience.enabled = true;
                exteriorAmbience.Play();
            }
        }
    }
}