using UnityEngine;
using System.Collections.Generic;

public class CustomerQueue : MonoBehaviour
{
    public List<NpcCustomer> customersInShop = new List<NpcCustomer>();
    
    // NEW: Track customers served for the dog sitting text trigger
    private int customersServed = 0;
    public int customersBeforeTextMessage = 1; // Trigger after 1st customer
    
    void Start()
    {
        Debug.Log($"[Queue] Starting with {customersInShop.Count} customers");
        
        if (customersInShop.Count > 0)
        {
            Debug.Log($"[Queue] Calling first customer: {customersInShop[0].name}");
            customersInShop[0].CallToCounter();
        }
    }

    public void CustomerLeft(NpcCustomer npc)
    {
        Debug.Log($"[Queue] CustomerLeft called for: {npc.name}");
        
        // NEW: Increment counter when a customer leaves
        customersServed++;
        Debug.Log($"[Queue] Total customers served: {customersServed}");
        
        if (customersInShop.Contains(npc))
        {
            customersInShop.Remove(npc);
            Debug.Log($"[Queue] Removed {npc.name}. Remaining customers: {customersInShop.Count}");
        }
        else
        {
            Debug.LogWarning($"[Queue] {npc.name} was not in the queue!");
        }

        // NEW: Check if we should show the dog sitting text message
        if (customersServed == customersBeforeTextMessage)
        {
            Debug.Log($"[Queue] {customersBeforeTextMessage}th customer served! Showing dog sitting text...");
            ShowDogSitTextMessage();
            return; // Exit early - don't call next customer yet (phone takes over)
        }

        // EXISTING: Call next customer as normal
        if (customersInShop.Count > 0)
        {
            Debug.Log($"[Queue] Calling next customer: {customersInShop[0].name}");
            customersInShop[0].CallToCounter();
        }
        else
        {
            Debug.Log("[Queue] No more customers waiting");
        }
    }

    // NEW: Show the dog sitting text message
    void ShowDogSitTextMessage()
    {
        if (PhoneManager.Instance == null)
        {
            Debug.LogError("[Queue] PhoneManager.Instance is NULL!");
            // Fallback: just call next customer if phone manager isn't available
            if (customersInShop.Count > 0)
                customersInShop[0].CallToCounter();
            return;
        }

        // Show the text message with callbacks
        PhoneManager.Instance.ReceiveTextMessage(
            // When player accepts:
            onAccept: () =>
            {
                Debug.Log("[Queue] Player accepted dog sitting! Story will continue...");
                // Resume queue - call next customer
                if (customersInShop.Count > 0)
                {
                    Debug.Log($"[Queue] Resuming queue, calling: {customersInShop[0].name}");
                    customersInShop[0].CallToCounter();
                }
            },
            // When player declines:
            onDecline: () =>
            {
                Debug.Log("[Queue] Player declined dog sitting. Continuing normal shift...");
                // Resume queue - call next customer
                if (customersInShop.Count > 0)
                {
                    Debug.Log($"[Queue] Resuming queue, calling: {customersInShop[0].name}");
                    customersInShop[0].CallToCounter();
                }
            }
        );
    }
}