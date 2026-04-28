using UnityEngine;

public class BarkTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource barkAudio;
    [SerializeField] private AudioClip barkClip;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            barkAudio.PlayOneShot(barkClip);
            gameObject.SetActive(false); // fire once then disable
        }
    }
}