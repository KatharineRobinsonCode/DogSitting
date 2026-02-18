using UnityEngine;
using TMPro;
using System.Collections;
/// <summary>
/// Handles customer order fulfillment at the cash register.
/// Validates drinks against customer orders and manages order completion.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Register : MonoBehaviour
{
#region Serialized Fields
[Header("Current Customer")]
[Tooltip("NPC currently being served (assigned by customer when they arrive)")]
public NpcCustomer currentCustomer;

[Header("Queue Management")]
[Tooltip("Reference to the customer queue manager")]
[SerializeField] private CustomerQueue queueManager;

[Header("Audio Feedback")]
[Tooltip("Sound played when order is correct")]
[SerializeField] private AudioClip successSound;

[Tooltip("Sound played when wrong drink is served")]
[SerializeField] private AudioClip errorSound;

#endregion

#region Private Fields

private AudioSource audioSource;

#endregion

#region Unity Lifecycle

private void Start()
{
    InitializeComponents();
    ValidateReferences();
}

#endregion

#region Initialization

private void InitializeComponents()
{
    audioSource = GetComponent<AudioSource>();
}

private void ValidateReferences()
{
    if (queueManager == null)
    {
        Debug.LogWarning("[Register] Queue Manager not assigned. Order completion may not work properly.");
    }
}

#endregion

#region Public API

/// <summary>
/// Called by PlayerInteraction when player presses E while looking at register.
/// Validates and processes drink delivery to current customer.
/// </summary>
public void Interact(PlayerInteraction player)
{
    // Validation chain - fail fast
    if (!ValidatePlayerHoldingItem(player)) return;
    if (!ValidateCupContents(player, out Cup cup)) return;
    if (!ValidateCustomerPresent()) return;
    
    // Process the drink delivery
    ProcessDrinkDelivery(player, cup);
}

#endregion

#region Validation

private bool ValidatePlayerHoldingItem(PlayerInteraction player)
{
    if (player.CurrentHeldItem != null)
    {
        return true;
    }
    
    ShowFeedback("You need to hold a drink first!", FeedbackManager.MessageType.Error);
    return false;
}

private bool ValidateCupContents(PlayerInteraction player, out Cup cup)
{
    cup = player.CurrentHeldItem.GetComponent<Cup>();
    
    if (cup == null)
    {
        ShowFeedback("That's not a cup!", FeedbackManager.MessageType.Error);
        return false;
    }
    
    if (cup.contents == Cup.DrinkType.None)
    {
        ShowFeedback("This cup is empty!", FeedbackManager.MessageType.Error);
        return false;
    }
    
    return true;
}

private bool ValidateCustomerPresent()
{
    if (currentCustomer != null)
    {
        return true;
    }
    
    ShowFeedback("No customer here right now.", FeedbackManager.MessageType.Info);
    return false;
}

#endregion

#region Order Processing

private void ProcessDrinkDelivery(PlayerInteraction player, Cup cup)
{
    string drinkName = cup.contents.ToString();
    string orderText = currentCustomer.FinalOrderToDisplay;  // ✅ FIXED - Using property
    
    Debug.Log($"[Register] Processing delivery: Customer={currentCustomer.name}, " +
              $"Order='{orderText}', Drink='{drinkName}'");
    
    if (IsCorrectDrink(drinkName, orderText))
    {
        HandleCorrectDrink(player);
    }
    else
    {
        HandleWrongDrink(orderText);
    }
}

private bool IsCorrectDrink(string drinkName, string orderText)
{
    // Case-insensitive comparison
    return orderText.ToLower().Contains(drinkName.ToLower());
}

#endregion

#region Correct Drink Handling

private void HandleCorrectDrink(PlayerInteraction player)
{
    PlaySuccessSound();
    RemoveCupFromPlayer(player);
    DeliverItemToCustomer();
    
    if (IsOrderComplete())
    {
        CompleteOrder();
    }
    else
    {
        ShowPartialOrderFeedback();
    }
}

private void PlaySuccessSound()
{
    if (audioSource != null && successSound != null)
    {
        audioSource.PlayOneShot(successSound);
    }
}

private void RemoveCupFromPlayer(PlayerInteraction player)
{
    if (player.CurrentHeldItem != null)
    {
        Destroy(player.CurrentHeldItem);
    }
}

private void DeliverItemToCustomer()
{
    if (currentCustomer != null)
    {
        currentCustomer.DeliverItem();
    }
}

private bool IsOrderComplete()
{
    return currentCustomer != null && 
           currentCustomer.ItemsReceived >= currentCustomer.ItemsExpected;  // ✅ FIXED - Using properties
}

private void CompleteOrder()
{
    Debug.Log($"[Register] Order complete for {currentCustomer.name}");
    
    NotifyQueueManager();
    ClearCurrentCustomer();
    ShowOrderCompleteFeedback();
}

private void NotifyQueueManager()
{
    if (queueManager != null && currentCustomer != null)
    {
        Debug.Log($"[Register] Notifying queue: {currentCustomer.name} finished");
        queueManager.CustomerLeft(currentCustomer);
    }
    else if (queueManager == null)
    {
        Debug.LogError("[Register] Cannot notify queue: QueueManager is null!");
    }
}

private void ClearCurrentCustomer()
{
    if (currentCustomer != null)
    {
        Debug.Log($"[Register] Clearing current customer: {currentCustomer.name}");
        currentCustomer = null;
    }
}

private void ShowPartialOrderFeedback()
{
    int remaining = currentCustomer.ItemsExpected - currentCustomer.ItemsReceived;  // ✅ FIXED - Using properties
    ShowFeedback(
        $"Nice! {remaining} more item(s) to serve.", 
        FeedbackManager.MessageType.Success
    );
}

private void ShowOrderCompleteFeedback()
{
    ShowFeedback("Order Complete!", FeedbackManager.MessageType.Success);
}

#endregion

#region Wrong Drink Handling

private void HandleWrongDrink(string expectedOrder)
{
    PlayErrorSound();
    ShowWrongDrinkFeedback(expectedOrder);
}

private void PlayErrorSound()
{
    if (audioSource != null && errorSound != null)
    {
        audioSource.PlayOneShot(errorSound);
    }
}

private void ShowWrongDrinkFeedback(string expectedOrder)
{
    ShowFeedback(
        $"Wrong Drink! They want {expectedOrder}", 
        FeedbackManager.MessageType.Error
    );
}

#endregion

#region Feedback Helpers

private void ShowFeedback(string message, FeedbackManager.MessageType type)
{
    FeedbackManager.Instance?.ShowMessage(message, type);
}

#endregion

#region Public Utility Methods

/// <summary>
/// Checks if register is currently occupied by a customer
/// </summary>
public bool IsOccupied()
{
    return currentCustomer != null;
}

/// <summary>
/// Manually clears the current customer (use with caution)
/// </summary>
public void ForceReleaseCustomer()
{
    if (currentCustomer != null)
    {
        Debug.LogWarning($"[Register] Force releasing customer: {currentCustomer.name}");
        currentCustomer = null;
    }
}

#endregion
}