using UnityEngine;

public class BarkTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource barkAudio;
    [SerializeField] private AudioClip barkClip;
    private bool hasBarked = false;

private void OnTriggerEnter(Collider other)
{
    if (hasBarkd) return;
    if (!other.CompareTag("Player")) return;
    if (HouseSceneState.skipBarkTrigger) return;
            hasBarked = true;
            barkAudio.PlayOneShot(barkClip);

            if (TaskManager.Instance != null)
                TaskManager.Instance.CompleteTask();
        }
}