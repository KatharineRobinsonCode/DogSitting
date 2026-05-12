using UnityEngine;

public class BarkTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource barkAudio;
    [SerializeField] private AudioClip barkClip;
    private bool hasBarked = false;

    private void Awake()
    {
        // If returning from mini game, mark as already barked
        if (HouseSceneState.isReturningFromMiniGame)
        {
            hasBarked = true;
            Debug.Log("[BarkTrigger] Returning from mini game — skipping bark");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBarked) return;
        if (!other.CompareTag("Player")) return;
        if (HouseSceneState.skipBarkTrigger) return;

        hasBarked = true;
        barkAudio.PlayOneShot(barkClip);

        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask();
    }
}