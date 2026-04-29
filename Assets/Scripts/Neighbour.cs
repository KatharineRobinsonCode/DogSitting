using UnityEngine;

public class Neighbour : MonoBehaviour
{
    [SerializeField] private AudioSource stepAudio;
    [SerializeField] private AudioClip stepClip;
    private bool hasStepped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasStepped) return;
        if (other.CompareTag("Player"))
        {
            hasStepped = true;
            stepAudio.PlayOneShot(stepClip);
        }
    }
}