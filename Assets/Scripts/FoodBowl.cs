using UnityEngine;

public class FoodBowl : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioSource bowlAudio;
    [SerializeField] private AudioClip fillSound;

    [Header("Visual (optional)")]
    [SerializeField] private GameObject emptyBowl;
    [SerializeField] private GameObject fullBowl;

    [Header("Dog")]
[SerializeField] private Dog dog;

    private bool isFilled = false;

    public string GetInteractionPrompt()
    {
        return isFilled ? "" : "Press E to fill Brinkley's bowl";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isFilled) return;
        isFilled = true;

        if (bowlAudio != null && fillSound != null)
            bowlAudio.PlayOneShot(fillSound);

        // Swap bowl visuals if assigned
        if (emptyBowl != null) emptyBowl.SetActive(false);
        if (fullBowl != null) fullBowl.SetActive(true);

        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask();

            if (dog != null)
    dog.StopFollowing();
    }
}