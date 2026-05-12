using UnityEngine;

public class ExteriorAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource exteriorAmbience;
private void Awake()
{
    Debug.Log("[ExteriorAudio] Awake — isReturning: " + HouseSceneState.isReturningFromMiniGame + 
              " audioSource null: " + (exteriorAmbience == null));
    
    if (HouseSceneState.isReturningFromMiniGame)
    {
        if (exteriorAmbience != null)
        {
            exteriorAmbience.Stop();
            exteriorAmbience.enabled = false;
            Debug.Log("[ExteriorAudio] Stopped and disabled");
        }
    }
}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (HouseSceneState.skipExteriorAudio) return;

        if (!exteriorAmbience.enabled)
            exteriorAmbience.enabled = true;

        exteriorAmbience.Play();
    }
}