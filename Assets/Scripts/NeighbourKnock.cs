using UnityEngine;

public class NeighbourKnock : MonoBehaviour, IInteractable
{
    [Header("Knocking")]
    [SerializeField] private AudioSource knockAudio;
    [SerializeField] private AudioClip knockClip;

    public string GetInteractionPrompt()
    {
        return "Press E to knock";
    }

    public void Interact(PlayerInteraction player)
    {
        if (knockAudio != null && knockClip != null)
            knockAudio.PlayOneShot(knockClip);
    }
}