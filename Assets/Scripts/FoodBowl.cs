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
[SerializeField] private Transform closetTarget;

    private bool isFilled = false;

public string GetInteractionPrompt()
{
    if (isFilled) return "";
    if (!InventoryManager.Instance.HasItem(ItemType.DogFood))
        return "You need dog food first";
    return "Press E to fill Brinkley's bowl";
}

public void Interact(PlayerInteraction player)
{
    if (isFilled) return;

    // Check player has dog food
    if (!InventoryManager.Instance.HasItem(ItemType.DogFood))
    {
        FeedbackManager.Instance?.ShowMessage("You need to find dog food first", 
            FeedbackManager.MessageType.Info);
        return;
    }

    isFilled = true;

    if (bowlAudio != null && fillSound != null)
        bowlAudio.PlayOneShot(fillSound);

    if (emptyBowl != null) emptyBowl.SetActive(false);
    if (fullBowl != null) fullBowl.SetActive(true);

    if (TaskManager.Instance != null)
        TaskManager.Instance.CompleteTask();
}

public void OnPizzaOrdered()
{
      Debug.Log("[FoodBowl] OnPizzaOrdered called");
    Debug.Log("[FoodBowl] dog is null: " + (dog == null));
    Debug.Log("[FoodBowl] closetTarget is null: " + (closetTarget == null));
    Debug.Log("[FoodBowl] fullBowl is null: " + (fullBowl == null));
    
    // Hide the food
    if (fullBowl != null) fullBowl.SetActive(false);
    
    // Send Brinkley to closet
    if (dog != null && closetTarget != null)
        dog.GoToPosition(closetTarget);
}
}