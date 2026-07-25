using UnityEngine;
using UnityEngine.AI;
using Yarn.Unity;
using System.Collections;

public class NpcCustomer : MonoBehaviour, IInteractable
{
    // ==========================
    // MOVEMENT SETTINGS
    // ==========================
    [Header("Movement")]
    public float runSpeed = 7f;
    public float walkSpeed = 3.5f;

    public Transform counterTarget;
    public Transform exitPoint;
    public Transform seatTarget;
    public bool shouldSitAtTable = false;
  [SerializeField] protected float turnSpeed = 7f;

    // Changed from private to protected so DrunkCustomer can access
    protected NavMeshAgent agent;
    protected Animator anim;

    protected bool isLeaving = false;
    protected bool isHeadingToSeat = false;
    public bool isWaiting = true;
    protected bool hasArrivedAtCounter = false;
    protected bool isThisNPCActing = false;

    // ==========================
    // DIALOGUE SETTINGS
    // ==========================
    [Header("Dialogue Config")]
    public GameObject interactionBubble;
    public float interactDistance = 3f;

    protected bool hasFinishedWaitingConversation = false;
    protected bool hasFinishedOrderConversation = false;

    public string waitingYarnNodeName = "Customer1_Waiting";
    public string counterYarnNodeName = "Customer1_Order";

    [Header("Yarn Commands (Make These Unique!)")]
    public string waitingCommandName = "CompleteWaitingConversation_Customer1";
    public string orderCommandName = "CompleteOrderConversation_Customer1";

    // ==========================
    // OTHER SCENE OBJECTS
    // ==========================
    [Header("Scene References")]
    public Transform player;
    public GameObject dialogueCanvas;
    public Register register;
    public Transform counterLookTarget;

    protected DialogueRunner dialogueRunner;

    // ==========================
    // ORDER INFORMATION
    // ==========================
    [Header("Order Info")]
    public string finalOrderToDisplay = "1x Draft Beer";

    [Header("Multi-Order Settings")]
    public int itemsExpected = 1;
    public int itemsReceived = 0;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip scaryRunSound;
    protected bool hasBeenServed = false;

    [Header("Audio Settings")]
    public float audioDelay = 0.5f;

    [Header("Spawn Settings")]
    public bool hideUntilCalled = false;
    public Transform spawnPoint;

    // Properties for Register.cs compatibility
    public string FinalOrderToDisplay => finalOrderToDisplay;
    public int ItemsReceived => itemsReceived;
    public int ItemsExpected => itemsExpected;

[Header("Task Gating")]
[SerializeField] private bool requiresServeTaskToOrder = false;
    // Changed to protected virtual so DrunkCustomer can override
    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.updateRotation = false;

        anim = GetComponentInChildren<Animator>();

        StartCoroutine(LateFindDialogueRunner());

        if (interactionBubble != null)
            interactionBubble.SetActive(false);

        if (!isWaiting)
            CallToCounter();

        if (hideUntilCalled)
            SetNPCVisibility(false);
    }

    IEnumerator LateFindDialogueRunner()
    {
        yield return new WaitForEndOfFrame();
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (dialogueRunner != null)
        {
            dialogueRunner.AddCommandHandler(waitingCommandName, CompleteWaitingConversation);
            dialogueRunner.AddCommandHandler(orderCommandName, CompleteOrderConversation);

            // Hook for subclasses to register their own commands
            RegisterAdditionalYarnCommands(dialogueRunner);

            Debug.Log($"[{name}] Registered commands: {waitingCommandName}, {orderCommandName}");

            Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
            if (canvasComponent != null)
            {
                dialogueCanvas = canvasComponent.gameObject;
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasGroup group = dialogueCanvas.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
            }
        }
    }

    // Virtual hook — base does nothing, DrunkCustomer overrides to add extra commands
    protected virtual void RegisterAdditionalYarnCommands(DialogueRunner runner) { }

    protected virtual void Update()
{
    if (anim != null && agent != null && agent.enabled)
        anim.SetFloat("Speed", agent.velocity.magnitude);

    HandleMovementAndArrival();

    if (agent != null && agent.enabled && agent.velocity.sqrMagnitude > 0.1f)
    {
        FaceTarget(transform.position + agent.velocity);
    }
    else if (player != null)
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (!isLeaving && !isHeadingToSeat && !hasFinishedWaitingConversation && dist <= interactDistance + 2f)
        {
            FaceTarget(player.position);

        if (isThisNPCActing)
{
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
}
        }
        else if (hasArrivedAtCounter && !isLeaving && !isHeadingToSeat && !isThisNPCActing)
        {
            Vector3 lookPos = (counterLookTarget != null ? counterLookTarget.position : counterTarget.position);
            FaceTarget(lookPos);
        }
    }
}
public virtual string GetInteractionPrompt()
{
    if (hasArrivedAtCounter && itemsReceived < itemsExpected)
    {
        // Order already taken — hide prompt while player makes the drink
        if (hasFinishedOrderConversation)
            return "";

        if (requiresServeTaskToOrder && 
            (TaskManager.Instance == null || !TaskManager.Instance.IsCurrentTask("Serve customers")))
        {
            if (!hasFinishedWaitingConversation)
                return "Press E to chat";
            return "";
        }
        return "Press E to take order";
    }

    if (!hasArrivedAtCounter)
    {
        if (!hasFinishedWaitingConversation)
            return "Press E to chat";
        return "";
    }

    return "";
}

public void Interact(PlayerInteraction player)
{
    dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    if (dialogueRunner == null || dialogueRunner.IsDialogueRunning) return;

    // If order is gated and task isn't serve customers, fire waiting conversation instead
   if (hasArrivedAtCounter && requiresServeTaskToOrder &&
    (TaskManager.Instance == null || !TaskManager.Instance.IsCurrentTask("Serve customers")))
{
    if (!hasFinishedWaitingConversation)
        StartCoroutine(StartDialogueNextFrame(waitingYarnNodeName));
    return;
}

    StartNpcDialogue();
}

    void StartNpcDialogue()
{
    dialogueRunner = FindFirstObjectByType<DialogueRunner>();

    if (dialogueRunner != null)
    {
        string nodeToStart = hasArrivedAtCounter ? counterYarnNodeName : waitingYarnNodeName;
Debug.Log($"[{name}] Attempting to start node: '{nodeToStart}' — hasArrivedAtCounter: {hasArrivedAtCounter}");
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        isThisNPCActing = true;
        StartCoroutine(StartDialogueNextFrame(nodeToStart));
    }
    else
    {
        Debug.LogError($"[{name}] can't find the DialogueRunner!");
    }
}

    IEnumerator StartDialogueNextFrame(string nodeName)
    {
        yield return null;
        if (dialogueRunner != null)
            dialogueRunner.StartDialogue(nodeName);
    }

    public void CompleteWaitingConversation()
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("CompleteWaitingConversation called on destroyed NPC");
            return;
        }

        Debug.Log($"[{name}] CompleteWaitingConversation called");
        hasFinishedWaitingConversation = true;
        isThisNPCActing = false;
        HideUIElements();
         Cursor.visible = false;       
    Cursor.lockState = CursorLockMode.Locked; 
    }

    public void CompleteOrderConversation()
    {
        if (this == null || gameObject == null) return;

        Debug.Log($"[{name}] CompleteOrderConversation called");
        Debug.Log($"[{name}] TaskManager.Instance is: {(TaskManager.Instance == null ? "NULL" : TaskManager.Instance.name)}");

        hasFinishedOrderConversation = true;

        if (OrderManager.Instance != null)
            OrderManager.Instance.ShowOrder("Order: " + finalOrderToDisplay);

        if (TaskManager.Instance != null)
            TaskManager.Instance.ShowTask("Make " + finalOrderToDisplay);
        else
            Debug.LogError($"[{name}] TaskManager.Instance is NULL! Can't update task.");

        HideUIElements();
        Cursor.visible = false;           
    Cursor.lockState = CursorLockMode.Locked; 
    }

    public void DeliverItem()
    {
        itemsReceived++;
        int remaining = itemsExpected - itemsReceived;

        Debug.Log($"[NpcCustomer] {name} received an item. {remaining} remaining.");

        if (itemsReceived >= itemsExpected)
        {
            isThisNPCActing = false;
            hasFinishedOrderConversation = true;
            HideUIElements();
            FinishOrderAndLeave();
        }
        else
        {
            if (OrderManager.Instance != null)
            {
                string updatedText = $"{finalOrderToDisplay} (Waiting for {remaining} more)";
                OrderManager.Instance.ShowOrder("Order: " + updatedText);
            }
        }
    }

    protected void HideUIElements()
    {
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (dialogueRunner != null)
        {
            CanvasGroup optionsGroup = dialogueRunner.GetComponentInChildren<CanvasGroup>(true);
            if (optionsGroup != null)
                optionsGroup.gameObject.SetActive(false);
        }
    }

    void HandleMovementAndArrival()
    {
        if (isLeaving)
        {
            if (exitPoint != null && Vector3.Distance(transform.position, exitPoint.position) < 1.5f)
                Destroy(gameObject);
            return;
        }

        if (isHeadingToSeat && seatTarget != null)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            {
                isHeadingToSeat = false;
                agent.isStopped = true;
                transform.rotation = seatTarget.rotation;

                if (anim != null)
                    anim.SetBool("isSitting", true);
            }
            return;
        }

    if (!isWaiting && !hasArrivedAtCounter && !hasBeenServed && agent != null && agent.enabled && counterTarget != null)
{
    if (agent.remainingDistance <= agent.stoppingDistance + 0.1f && !agent.pathPending)
    {
                hasArrivedAtCounter = true;
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;

                OnArrivedAtCounter(); // ← virtual hook for DrunkCustomer

                Vector3 lookPos = counterLookTarget != null ? counterLookTarget.position : counterTarget.position;
                Vector3 direction = (lookPos - transform.position).normalized;
                direction.y = 0;

                if (direction != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(direction);

                if (register != null)
                {
                    Debug.Log($"[{name}] Setting register.currentCustomer to {name}");
                    register.currentCustomer = this;
                }
            }
        }
    }

    // Virtual hook — base does nothing, DrunkCustomer overrides to switch animation
    protected virtual void OnArrivedAtCounter() { }

    public virtual void FinishOrderAndLeave()
    {
        Debug.Log($"[{name}] FinishOrderAndLeave() called!");
        string seatName = (seatTarget != null) ? seatTarget.name : "None";
        Debug.Log($"[{name}] shouldSitAtTable={shouldSitAtTable}, seatTarget={seatName}");

        hasBeenServed = true;

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.CustomerLeft();
            OrderManager.Instance.HideOrder();
        }

        hasArrivedAtCounter = false;
        Debug.Log($"[{name}] Set hasArrivedAtCounter to FALSE");

        if (agent != null)
            agent.speed = walkSpeed;

        if (shouldSitAtTable && seatTarget != null)
        {
            Debug.Log($"[{name}] Going to sit at: {seatTarget.position}");
            isHeadingToSeat = true;
            MoveTo(seatTarget.position);
        }
        else
        {
            Debug.Log($"[{name}] Leaving to exit point");
            isLeaving = true;
            MoveTo(exitPoint.position);
        }
    }

    void MoveTo(Vector3 pos)
    {
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.SetDestination(pos);
        }
    }

    void FaceTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }

    public virtual void CallToCounter()
    {
        if (spawnPoint != null && agent != null)
        {
            agent.enabled = false;
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            agent.enabled = true;
            agent.Warp(spawnPoint.position);
        }

        SetNPCVisibility(true);
        isWaiting = false;

        if (agent != null && counterTarget != null)
        {
            agent.speed = runSpeed;
            agent.SetDestination(counterTarget.position);
            StartCoroutine(PlayScarySoundWithDelay());
        }
    }

    private IEnumerator PlayScarySoundWithDelay()
    {
        yield return new WaitForSeconds(audioDelay);

        if (audioSource != null && scaryRunSound != null)
            audioSource.PlayOneShot(scaryRunSound);
    }

    public void SetNPCVisibility(bool isVisible)
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
            r.enabled = isVisible;

        if (interactionBubble != null)
            interactionBubble.SetActive(isVisible);
    }

    public void ForceLeave()
    {
        Debug.Log($"[{name}] ForceLeave called - heading to exit");
        hasBeenServed = true;
        isHeadingToSeat = false;
        isLeaving = true;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.speed = walkSpeed;
            agent.SetDestination(exitPoint.position);
        }

        if (anim != null)
            anim.SetBool("isSitting", false);
    }
    private void OnDestroy()
{
    if (dialogueRunner == null) return;
    
    try { dialogueRunner.RemoveCommandHandler(waitingCommandName); } catch { }
    try { dialogueRunner.RemoveCommandHandler(orderCommandName); } catch { }
}
}