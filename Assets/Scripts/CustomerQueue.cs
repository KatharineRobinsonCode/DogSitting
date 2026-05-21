using UnityEngine;
using System.Collections.Generic;

public class CustomerQueue : MonoBehaviour
{
    public List<NpcCustomer> customersInShop = new List<NpcCustomer>();
    
    private int customersServed = 0;
    public int customersBeforeTextMessage = 1;

    // New pause flag
    private bool isPaused = false;

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
        
        customersServed++;
        Debug.Log($"[Queue] Total customers served: {customersServed}");
          
        OrderManager.Instance?.CustomerLeft();
        
        if (customersInShop.Contains(npc))
        {
            customersInShop.Remove(npc);
            Debug.Log($"[Queue] Removed {npc.name}. Remaining: {customersInShop.Count}");
        }
        else
        {
            Debug.LogWarning($"[Queue] {npc.name} was not in the queue!");
        }

        // Dog sit text message trigger
        if (customersServed == customersBeforeTextMessage)
        {
            Debug.Log($"[Queue] Showing dog sitting text...");
            ShowDogSitTextMessage();
            return;
        }

        // Toilet task trigger after 3rd customer
        if (customersServed == 3)
        {
            Debug.Log($"[Queue] 3rd customer served — triggering toilet check");
            isPaused = true;
            CoffeeShopManager.Instance?.OnThirdCustomerServed();
            return;
        }

        CallNextCustomer();
    }

    public void ResumeQueue()
    {
        isPaused = false;
        Debug.Log("[Queue] Resuming queue");
        CallNextCustomer();
    }

    private void CallNextCustomer()
    {
        if (isPaused) return;

        if (customersInShop.Count > 0)
        {
            Debug.Log($"[Queue] Calling next customer: {customersInShop[0].name}");
            customersInShop[0].CallToCounter();
        }
        else
        {
            Debug.Log("[Queue] No more customers");
            CoffeeShopManager.Instance?.OnAllCustomersServed();
        }
    }

    void ShowDogSitTextMessage()
    {
        if (PhoneManager.Instance == null)
        {
            Debug.LogError("[Queue] PhoneManager.Instance is NULL!");
            CallNextCustomer();
            return;
        }

        PhoneManager.Instance.ReceiveTextMessage(
            onAccept: () =>
            {
                Debug.Log("[Queue] Player accepted dog sitting!");
                CallNextCustomer();
            },
            onDecline: () =>
            {
                Debug.Log("[Queue] Player declined dog sitting.");
                CallNextCustomer();
            }
        );
    }
}