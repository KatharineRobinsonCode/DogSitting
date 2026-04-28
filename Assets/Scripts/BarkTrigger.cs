using UnityEngine;

public class BarkTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource barkAudio;
    [SerializeField] private AudioClip barkClip;

    private void OnTriggerEnter(Collider other)
{
    Debug.Log($"[BarkTrigger] Hit by: {other.gameObject.name} tag: {other.tag}");
    if (other.CompareTag("Player"))
    {
        barkAudio.PlayOneShot(barkClip);
        gameObject.SetActive(false);
    }
}
}