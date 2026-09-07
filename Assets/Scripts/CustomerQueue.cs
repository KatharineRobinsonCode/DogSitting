using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Yarn.Unity;

public class CustomerQueue : MonoBehaviour
{
    public List<NpcCustomer> customersInShop = new List<NpcCustomer>();
    
    private int customersServed = 0;
    public int customersBeforeTextMessage = 1;

    [Header("Delays")]
    [SerializeField] private float customerDelay = 10f;

    [Header("Beer Mat Game")]
    [SerializeField] private BeerMatStackGame beerMatStackGame;
    [SerializeField] private Transform beerMatPileLocation;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform playerBody;
    [SerializeField] private Camera playerCam;
    [SerializeField] private float zoomFOV = 45f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private string beerMatMonologueNode = "BeerMatThought";

    private bool isPaused = false;
    private DialogueRunner dialogueRunner;

    void Start()
    {
        Debug.Log($"[Queue] Starting with {customersInShop.Count} customers");
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        
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

        // Carol text after customer 1
        if (customersServed == customersBeforeTextMessage)
        {
            Debug.Log($"[Queue] Showing dog sitting text...");
            ShowDogSitTextMessage();
            return;
        }

        // After customer 2 — delay before next
        if (customersServed == 2)
        {
            StartCoroutine(DelayedNextCustomer(customerDelay));
            return;
        }

        // After customer 3 — trigger beer mat stack game
        if (customersServed == 3)
        {
            StartCoroutine(TriggerBeerMatGame());
            return;
        }

        // After customer 4 — toilet check
        if (customersServed == 4)
        {
            Debug.Log($"[Queue] 4th customer served — triggering toilet check");
            isPaused = true;
            CoffeeShopManager.Instance?.OnThirdCustomerServed();
            return;
        }

        // After customer 5 — delay before next
        if (customersServed == 5)
        {
            StartCoroutine(DelayedNextCustomer(customerDelay));
            return;
        }

        // After customer 6 — delay then all served
        if (customersServed == 6)
        {
            StartCoroutine(DelayedAllServed(customerDelay));
            return;
        }

        CallNextCustomer();
    }

    private IEnumerator DelayedNextCustomer(float delay)
    {
        Debug.Log($"[Queue] Waiting {delay}s before next customer");
        yield return new WaitForSeconds(delay);
        CallNextCustomer();
    }

    private IEnumerator DelayedAllServed(float delay)
    {
        Debug.Log($"[Queue] Waiting {delay}s before all served");
        yield return new WaitForSeconds(delay);
        CoffeeShopManager.Instance?.OnAllCustomersServed();
    }

    private IEnumerator TriggerBeerMatGame()
    {
        // Pause queue while game plays out
        isPaused = true;

        // Disable player controls
        var mouseLook = playerCamera?.GetComponent<SojaExiles.MouseLook>();
        var playerMovement = playerBody?.GetComponent<PlayerMovement>();
        if (mouseLook != null) mouseLook.enabled = false;
        if (playerMovement != null) playerMovement.SetMovementEnabled(false);

        yield return new WaitForSeconds(1f);

        // Cut camera to beer mat pile
        if (beerMatPileLocation != null && playerCamera != null)
        {
            Vector3 direction = (beerMatPileLocation.position - playerCamera.position).normalized;
            Vector3 flatDir = new Vector3(direction.x, 0f, direction.z);
            if (flatDir != Vector3.zero)
                playerBody.rotation = Quaternion.LookRotation(flatDir);
            float vertAngle = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
            playerCamera.localRotation = Quaternion.Euler(-vertAngle, 0f, 0f);
        }

        // Zoom in
        if (playerCam != null)
        {
            float elapsed = 0f;
            float start = playerCam.fieldOfView;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                playerCam.fieldOfView = Mathf.Lerp(start, zoomFOV, elapsed / 0.5f);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.8f);

        // Restore controls
        if (mouseLook != null) mouseLook.enabled = true;
        if (playerMovement != null) playerMovement.SetMovementEnabled(true);

        // Zoom back out
        if (playerCam != null)
        {
            float elapsed = 0f;
            float start = playerCam.fieldOfView;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                playerCam.fieldOfView = Mathf.Lerp(start, normalFOV, elapsed / 0.5f);
                yield return null;
            }
        }

        // Internal monologue
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
            if (canvasComponent != null)
            {
                canvasComponent.gameObject.SetActive(true);
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasGroup group = canvasComponent.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
            }

            if (PauseManager.Instance != null)
                PauseManager.Instance.ShowCursorPublic();

            bool done = false;
            dialogueRunner.onDialogueComplete.AddListener(() => done = true);
            dialogueRunner.StartDialogue(beerMatMonologueNode);
            while (!done) yield return null;
            dialogueRunner.onDialogueComplete.RemoveListener(() => done = true);

            if (PauseManager.Instance != null)
                PauseManager.Instance.HideCursorPublic();
        }

        // Set task
        TaskManager.Instance?.ShowTask("Play a game with the beer mats");
        isPaused = false;
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
            NpcCustomer next = customersInShop[0];
            if (next != null)
                next.CallToCounter();
            else
                Debug.LogError("[Queue] Next customer is null!");
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