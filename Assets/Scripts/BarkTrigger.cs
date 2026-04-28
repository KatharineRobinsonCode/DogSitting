using UnityEngine;

public class BarkTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource barkAudio;
    [SerializeField] private AudioClip barkClip;
    private bool hasBarked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasBarked) return;
        if (other.CompareTag("Player"))
        {
            hasBarked = true;
            barkAudio.PlayOneShot(barkClip);
        }
    }
}