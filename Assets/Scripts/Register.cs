using UnityEngine;
using TMPro;
using System.Collections;

public class Register : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("NPC Settings")]
    public NpcCustomer currentCustomer;

    [Header("Queue Settings")]
    public CustomerQueue queueManager;

    [Header("Audio")]
    public AudioClip successSound;
    public AudioClip errorSound;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Called by PlayerInteraction when E is pressed on the register
    public void Interact(PlayerInteraction player)
    {
        // 1. Is the player holding anything?
        if (player.currentHeldItem == null)
        {
            FeedbackManager.Instance?.ShowMessage(
                "You need to hold a drink first!", 
                FeedbackManager.MessageType.Error
            );
            return;
        }

        // 2. Is it a Cup?
        Cup cup = player.currentHeldItem.GetComponent<Cup>();
        if (cup == null || cup.contents == Cup.DrinkType.None)
        {
            FeedbackManager.Instance?.ShowMessage(
                "This cup is empty!", 
                FeedbackManager.MessageType.Error
            );
            return;
        }

        // 3. Is there a customer to serve?
        if (currentCustomer == null)
        {
            FeedbackManager.Instance?.ShowMessage(
                "No customer here right now.", 
                FeedbackManager.MessageType.Info
            );
            return;
        }

        // 4. Convert the cup contents to a string we can match
        string heldDrinkName = cup.contents.ToString();
        string orderText     = currentCustomer.finalOrderToDisplay;
        string orderLower    = orderText.ToLower();
        string drinkLower    = heldDrinkName.ToLower();

        Debug.Log($"[Register] Serving customer: {currentCustomer.name} | " +
                  $"Order text: '{orderText}' | Held drink: '{heldDrinkName}'");

        // 5. Does this drink appear in their order text? (case‑insensitive)
        if (orderLower.Contains(drinkLower))
        {
            // ===== SUCCESS: This item is part of their order =====
            if (audioSource != null && successSound != null)
                audioSource.PlayOneShot(successSound);

            // Remove the physical cup from the world
            Destroy(player.currentHeldItem);
            player.currentHeldItem = null;

            // Tell the NPC they received one required item
            currentCustomer.DeliverItem();

            // If they are now fully satisfied, hand them back to the queue and clear slot
            if (currentCustomer.itemsExpected <= currentCustomer.itemsReceived)
            {
                Debug.Log($"[Register] Customer {currentCustomer.name} order complete!");
                
                // Queue handover
                if (queueManager != null)
                {
                    Debug.Log($"[Register] Calling queueManager.CustomerLeft({currentCustomer.name})");
                    queueManager.CustomerLeft(currentCustomer);
                }
                else
                {
                    Debug.LogError("[Register] QueueManager is NULL!");
                }

                // Free the register for the next customer
                Debug.Log($"[Register] Clearing currentCustomer (was {currentCustomer.name})");
                currentCustomer = null;
                Debug.Log("[Register] currentCustomer is now NULL");

                FeedbackManager.Instance?.ShowMessage(
                    "Order Complete!", 
                    FeedbackManager.MessageType.Success
                );
            }
            else
            {
                // Partial success – let the player know there's more to do
                int remaining = currentCustomer.itemsExpected - currentCustomer.itemsReceived;
                FeedbackManager.Instance?.ShowMessage(
                    $"Nice! {remaining} more item(s) to serve.", 
                    FeedbackManager.MessageType.Success
                );
            }
        }
        else
        {
            // ===== FAILURE: Wrong drink =====
            if (audioSource != null && errorSound != null)
                audioSource.PlayOneShot(errorSound);

            FeedbackManager.Instance?.ShowMessage(
                "Wrong Drink! They want " + currentCustomer.finalOrderToDisplay, 
                FeedbackManager.MessageType.Error
            );
        }
    }
}