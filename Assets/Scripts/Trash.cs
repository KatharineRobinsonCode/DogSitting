using UnityEngine;

public class Trash : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip trashSound;

    public void Interact(PlayerInteraction player)
    {
        if (player.currentHeldItem != null)
        {
            if (audioSource != null && trashSound != null)
                audioSource.PlayOneShot(trashSound);

            Destroy(player.currentHeldItem);
            player.currentHeldItem = null;

            FeedbackManager.Instance?.ShowMessage(
                "Drink discarded", 
                FeedbackManager.MessageType.Info
            );

            Debug.Log("Item trashed.");
        }
        else
        {
            FeedbackManager.Instance?.ShowMessage(
                "You're not holding anything!", 
                FeedbackManager.MessageType.Error
            );
        }
    }
}