using UnityEngine;
using UnityEngine.AI;
using Yarn.Unity;
using System.Collections;

/// <summary>
/// Manages NPC customer behavior including movement, dialogue, and order processing.
/// Can be configured for different customer types through the Inspector.
/// </summary>
public class NpcCustomer : MonoBehaviour, IInteractable
{
    #region Serialized Fields
    
    [Header("Movement Settings")]
    [Tooltip("Speed when rushing to counter")]
    [SerializeField] private float runSpeed = 7f;
    
    [Tooltip("Speed when leaving cafe")]
    [SerializeField] private float walkSpeed = 3.5f;
    
    [Tooltip("How quickly NPC rotates to face targets")]
    [SerializeField] private float turnSpeed = 7f;
    
    [Header("Movement Targets")]
    [SerializeField] private Transform counterTarget;
    [SerializeField] private Transform exitPoint;
    [SerializeField] private Transform seatTarget;
    [SerializeField] private Transform counterLookTarget;
    [SerializeField] private bool shouldSitAtTable = false;
    
    [Header("Dialogue Configuration")]
    [SerializeField] private GameObject interactionBubble;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private string waitingYarnNodeName = "Customer1_Waiting";
    [SerializeField] private string counterYarnNodeName = "Customer1_Order";
    
    [Header("Unique Yarn Command Names")]
    [Tooltip("Must be unique per customer (e.g., CompleteWaitingConversation_Customer1)")]
    [SerializeField] private string waitingCommandName = "CompleteWaitingConversation_Customer1";
    
    [Tooltip("Must be unique per customer (e.g., CompleteOrderConversation_Customer1)")]
    [SerializeField] private string orderCommandName = "CompleteOrderConversation_Customer1";
    
    [Header("Scene References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private Register register;
    
    [Header("Order Information")]
    [SerializeField] private string finalOrderToDisplay = "1x Coffee";
    [SerializeField] private int itemsExpected = 1;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scaryRunSound;
    [SerializeField] private float audioDelay = 0.5f;
    
    [Header("Spawn Settings")]
    [Tooltip("Hide NPC until CallToCounter() is invoked")]
    [SerializeField] private bool hideUntilCalled = false;
    [SerializeField] private Transform spawnPoint;
    
    #endregion
    
    #region Private Fields
    
    private NavMeshAgent agent;
    private Animator animator;
    private DialogueRunner dialogueRunner;
    
    // State tracking
    private int itemsReceived = 0;
    private bool isLeaving = false;
    private bool isHeadingToSeat = false;
    private bool isWaiting = true;
    private bool hasArrivedAtCounter = false;
    private bool hasBeenServed = false;
    private bool isInDialogue = false;
    private bool hasFinishedWaitingConversation = false;
    private bool hasFinishedOrderConversation = false;
    
    // Constants
    private const float ARRIVAL_THRESHOLD = 1.5f;
    private const float STOPPING_DISTANCE_BUFFER = 0.1f;
    private const float MOVEMENT_THRESHOLD = 0.1f;
    private const float EXTENDED_INTERACT_RANGE = 2f;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeComponents();
        SetupInitialState();
    }
    
    private void Update()
    {
        UpdateAnimation();
        HandleMovementAndArrival();
        UpdateFacingDirection();
    }
    
    #endregion

    #region Properties

/// <summary>
/// Public read-only access to final order text
/// </summary>
public string FinalOrderToDisplay => finalOrderToDisplay;

/// <summary>
/// Public read-only access to items expected in order
/// </summary>
public int ItemsExpected => itemsExpected;

/// <summary>
/// Public read-only access to items received so far
/// </summary>
public int ItemsReceived => itemsReceived;

#endregion
    
    #region Initialization
    
    private void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.updateRotation = false;
        }
        
        animator = GetComponentInChildren<Animator>();
        
        StartCoroutine(InitializeDialogueSystem());
    }
    
    private void SetupInitialState()
    {
        if (interactionBubble != null)
        {
            interactionBubble.SetActive(false);
        }
        
        if (!isWaiting)
        {
            CallToCounter();
        }
        
        if (hideUntilCalled)
        {
            SetNPCVisibility(false);
        }
    }
    
    private IEnumerator InitializeDialogueSystem()
    {
        yield return new WaitForEndOfFrame();
        
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        
        if (dialogueRunner != null)
        {
            RegisterDialogueCommands();
            SetupDialogueCanvas();
        }
        else
        {
            Debug.LogWarning($"[{name}] DialogueRunner not found in scene");
        }
    }
    
    private void RegisterDialogueCommands()
    {
        dialogueRunner.AddCommandHandler(waitingCommandName, CompleteWaitingConversation);
        dialogueRunner.AddCommandHandler(orderCommandName, CompleteOrderConversation);
        
        Debug.Log($"[{name}] Registered Yarn commands: {waitingCommandName}, {orderCommandName}");
    }
    
    private void SetupDialogueCanvas()
    {
        Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
        if (canvasComponent != null)
        {
            dialogueCanvas = canvasComponent.gameObject;
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasGroup group = dialogueCanvas.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
            }
        }
    }
    
    #endregion
    
    #region IInteractable Implementation
    
    public string GetInteractionPrompt()
    {
        if (hasArrivedAtCounter && itemsReceived < itemsExpected)
        {
            return "Press E to take order";
        }
        
        if (!hasArrivedAtCounter)
        {
            return "Press E to chat";
        }
        
        return string.Empty;
    }
    
    public void Interact(PlayerInteraction player)
    {
        if (CanStartDialogue())
        {
            StartNpcDialogue();
        }
    }
    
    #endregion
    
    #region Dialogue System
    
    private bool CanStartDialogue()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        return dialogueRunner != null && !dialogueRunner.IsDialogueRunning;
    }
    
    private void StartNpcDialogue()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(true);
        }
        
        isInDialogue = true;
        
        string nodeToStart = hasArrivedAtCounter ? counterYarnNodeName : waitingYarnNodeName;
        Debug.Log($"[{name}] Starting dialogue node: {nodeToStart}");
        
        StartCoroutine(StartDialogueNextFrame(nodeToStart));
    }
    
    private IEnumerator StartDialogueNextFrame(string nodeName)
    {
        yield return null;
        
        if (dialogueRunner != null)
        {
            dialogueRunner.StartDialogue(nodeName);
        }
    }
    
    private void CompleteWaitingConversation()
    {
        Debug.Log($"[{name}] Waiting conversation completed");
        
        hasFinishedWaitingConversation = true;
        isInDialogue = false;
        
        HideDialogueUI();
    }
    
    private void CompleteOrderConversation()
    {
        Debug.Log($"[{name}] Order conversation completed");
        
        hasFinishedOrderConversation = true;
        
        DisplayOrder();
        HideDialogueUI();
    }
    
    private void HideDialogueUI()
    {
        if (dialogueCanvas != null)
        {
            dialogueCanvas.SetActive(false);
        }
        
        if (dialogueRunner != null)
        {
            CanvasGroup optionsGroup = dialogueRunner.GetComponentInChildren<CanvasGroup>(true);
            if (optionsGroup != null)
            {
                optionsGroup.gameObject.SetActive(false);
            }
        }
    }
    
    #endregion
    
   #region Order Management

private void DisplayOrder()
{
    if (OrderManager.Instance != null)
    {
        OrderManager.Instance.ShowOrder($"Order: {finalOrderToDisplay}");
    }
}

private void UpdateTaskWithOrder() // ✨ NEW METHOD
{
    if (TaskManager.Instance != null)
    {
        TaskManager.Instance.ShowTask($"Serve customer: {finalOrderToDisplay}");
    }
}

public void DeliverItem()
{
    itemsReceived++;
    int remainingItems = itemsExpected - itemsReceived;
    
    Debug.Log($"[{name}] Received item. {remainingItems} remaining.");
    
    if (IsOrderComplete())
    {
        CompleteOrder();
    }
    else
    {
        UpdatePartialOrder(remainingItems);
    }
}

private bool IsOrderComplete()
{
    return itemsReceived >= itemsExpected;
}

private void CompleteOrder()
{
    isInDialogue = false;
    hasFinishedOrderConversation = true;
    
    HideDialogueUI();
    FinishOrderAndLeave();
}

private void UpdatePartialOrder(int remainingItems)
{
    if (OrderManager.Instance != null)
    {
        string updatedText = $"{finalOrderToDisplay} (Waiting for {remainingItems} more)";
        OrderManager.Instance.ShowOrder($"Order: {updatedText}");
    }
}

#endregion
    
    #region Movement System
    
    private void HandleMovementAndArrival()
    {
        if (isLeaving)
        {
            HandleLeaving();
            return;
        }
        
        if (isHeadingToSeat)
        {
            HandleSitting();
            return;
        }
        
        if (ShouldCheckCounterArrival())
        {
            CheckCounterArrival();
        }
    }
    
    private void HandleLeaving()
    {
        if (HasReachedExit())
        {
            Destroy(gameObject);
        }
    }
    
    private bool HasReachedExit()
    {
        return exitPoint != null && 
               Vector3.Distance(transform.position, exitPoint.position) < ARRIVAL_THRESHOLD;
    }
    
    private void HandleSitting()
    {
        if (seatTarget == null) return;
        
        if (HasReachedSeat())
        {
            CompleteSitting();
        }
    }
    
    private bool HasReachedSeat()
    {
        return !agent.pathPending && 
               agent.remainingDistance <= agent.stoppingDistance + STOPPING_DISTANCE_BUFFER;
    }
    
    private void CompleteSitting()
    {
        isHeadingToSeat = false;
        agent.isStopped = true;
        transform.rotation = seatTarget.rotation;
        
        if (animator != null)
        {
            animator.SetBool("isSitting", true);
        }
    }
    
    private bool ShouldCheckCounterArrival()
    {
        return !isWaiting && 
               !hasArrivedAtCounter && 
               !hasBeenServed && 
               agent != null && 
               counterTarget != null;
    }
    
    private void CheckCounterArrival()
    {
        if (HasReachedCounter())
        {
            ArriveAtCounter();
        }
    }
    
    private bool HasReachedCounter()
    {
        return agent.remainingDistance <= agent.stoppingDistance + STOPPING_DISTANCE_BUFFER && 
               !agent.pathPending;
    }
    
    private void ArriveAtCounter()
    {
        hasArrivedAtCounter = true;
        
        StopMovement();
        FaceCounterTarget();
        AssignToRegister();
    }
    
    private void StopMovement()
    {
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }
    
    private void FaceCounterTarget()
    {
        Vector3 lookPos = counterLookTarget != null ? 
                         counterLookTarget.position : 
                         counterTarget.position;
        
        Vector3 direction = (lookPos - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    private void AssignToRegister()
    {
        if (register != null)
        {
            Debug.Log($"[{name}] Assigned to register");
            register.currentCustomer = this;
        }
    }
    
    private void MoveTo(Vector3 destination)
    {
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.SetDestination(destination);
        }
    }
    
    public void CallToCounter()
    {
        TeleportToSpawnPoint();
        SetNPCVisibility(true);
        
        isWaiting = false;
        
        BeginRunToCounter();
    }
    
    private void TeleportToSpawnPoint()
    {
        if (spawnPoint != null && agent != null)
        {
            agent.enabled = false;
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            agent.enabled = true;
            agent.Warp(spawnPoint.position);
        }
    }
    
    private void BeginRunToCounter()
    {
        if (agent != null && counterTarget != null)
        {
            agent.speed = runSpeed;
            agent.SetDestination(counterTarget.position);
            StartCoroutine(PlayScarySoundWithDelay());
        }
    }
    
    public void FinishOrderAndLeave()
    {
        Debug.Log($"[{name}] Finishing order and leaving");
        
        hasBeenServed = true;
        hasArrivedAtCounter = false;
        
        NotifyOrderManager();
        
        if (agent != null)
        {
            agent.speed = walkSpeed;
        }
        
        DetermineDestination();
    }
    
    private void NotifyOrderManager()
    {
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.CustomerLeft();
            OrderManager.Instance.HideOrder();
        }
    }
    
    private void DetermineDestination()
    {
        if (shouldSitAtTable && seatTarget != null)
        {
            Debug.Log($"[{name}] Moving to seat");
            isHeadingToSeat = true;
            MoveTo(seatTarget.position);
        }
        else
        {
            Debug.Log($"[{name}] Leaving cafe");
            isLeaving = true;
            MoveTo(exitPoint.position);
        }
    }
    
    #endregion
    
    #region Animation & Facing
    
    private void UpdateAnimation()
    {
        if (animator != null && agent != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }
    
    private void UpdateFacingDirection()
    {
        if (IsMoving())
        {
            FaceMovementDirection();
        }
        else if (ShouldFacePlayer())
        {
            FacePlayer();
            ShowCursorIfInDialogue();
        }
        else if (ShouldFaceCounter())
        {
            FaceCounterTarget();
        }
    }
    
    private bool IsMoving()
    {
        return agent != null && agent.velocity.sqrMagnitude > MOVEMENT_THRESHOLD;
    }
    
    private void FaceMovementDirection()
    {
        FaceTarget(transform.position + agent.velocity);
    }
    
    private bool ShouldFacePlayer()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        return !isLeaving && 
               !isHeadingToSeat && 
               !hasFinishedWaitingConversation && 
               distanceToPlayer <= interactDistance + EXTENDED_INTERACT_RANGE;
    }
    
    private void FacePlayer()
    {
        FaceTarget(player.position);
    }
    
    private void ShowCursorIfInDialogue()
    {
        if (isInDialogue)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
    private bool ShouldFaceCounter()
    {
        return hasArrivedAtCounter && 
               !isLeaving && 
               !isHeadingToSeat && 
               !isInDialogue;
    }
    
    private void FaceTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                Time.deltaTime * turnSpeed
            );
        }
    }
    
    #endregion
    
    #region Audio
    
    private IEnumerator PlayScarySoundWithDelay()
    {
        yield return new WaitForSeconds(audioDelay);
        
        if (audioSource != null && scaryRunSound != null)
        {
            audioSource.PlayOneShot(scaryRunSound);
        }
    }
    
    #endregion
    
    #region Visibility
    
    public void SetNPCVisibility(bool isVisible)
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            renderer.enabled = isVisible;
        }
        
        if (interactionBubble != null)
        {
            interactionBubble.SetActive(isVisible);
        }
    }
    
    #endregion
}