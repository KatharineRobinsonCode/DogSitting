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

public float turnSpeed = 7f;



private NavMeshAgent agent;

private Animator anim;



private bool isLeaving = false;

private bool isHeadingToSeat = false;

public bool isWaiting = true;

private bool hasArrivedAtCounter = false;

private bool isThisNPCActing = false;



// ==========================

// DIALOGUE SETTINGS

// ==========================

[Header("Dialogue Config")]

public GameObject interactionBubble;

public float interactDistance = 3f;



private bool hasFinishedWaitingConversation = false;

private bool hasFinishedOrderConversation = false;



public string waitingYarnNodeName = "Customer1_Waiting";

public string counterYarnNodeName = "Customer1_Order";



// NEW: Unique command names for each customer

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



private DialogueRunner dialogueRunner;



// ==========================

// ORDER INFORMATION

// ==========================

[Header("Order Info")]

public string finalOrderToDisplay = "1x Coffee";



[Header("Multi-Order Settings")]

public int itemsExpected = 1;

public int itemsReceived = 0;



[Header("Audio")]

public AudioSource audioSource;

public AudioClip scaryRunSound;

private bool hasBeenServed = false;



[Header("Audio Settings")]

public float audioDelay = 0.5f;



[Header("Spawn Settings")]

public bool hideUntilCalled = false;

public Transform spawnPoint;



void Start()

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

{

SetNPCVisibility(false);

}

}



IEnumerator LateFindDialogueRunner()

{

yield return new WaitForEndOfFrame();

dialogueRunner = FindFirstObjectByType<DialogueRunner>();


if (dialogueRunner != null)

{

// Register commands with UNIQUE names

dialogueRunner.AddCommandHandler(waitingCommandName, CompleteWaitingConversation);

dialogueRunner.AddCommandHandler(orderCommandName, CompleteOrderConversation);



Debug.Log($"[{name}] Registered commands: {waitingCommandName}, {orderCommandName}");



// UI Setup

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



void Update()

{

if (anim != null && agent != null)

anim.SetFloat("Speed", agent.velocity.magnitude);



HandleMovementAndArrival();



if (agent != null && agent.velocity.sqrMagnitude > 0.1f)

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


return "";

}



public void Interact(PlayerInteraction player)

{

dialogueRunner = FindFirstObjectByType<DialogueRunner>();



if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)

{

StartNpcDialogue();

}

}



void StartNpcDialogue()

{

dialogueRunner = FindFirstObjectByType<DialogueRunner>();



if (dialogueRunner != null)

{

Debug.Log($"[{name}] Starting dialogue node: {(hasArrivedAtCounter ? counterYarnNodeName : waitingYarnNodeName)}");


if (dialogueCanvas != null)

dialogueCanvas.SetActive(true);



isThisNPCActing = true;

string nodeToStart = hasArrivedAtCounter ? counterYarnNodeName : waitingYarnNodeName;

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



// These methods are called by Yarn via AddCommandHandler

// NO [YarnCommand] attribute needed anymore!

public void CompleteWaitingConversation()

{

Debug.Log($"[{name}] CompleteWaitingConversation called");

hasFinishedWaitingConversation = true;

isThisNPCActing = false;

HideUIElements();

}



public void CompleteOrderConversation()

{

Debug.Log($"[{name}] CompleteOrderConversation called");

hasFinishedOrderConversation = true;


if (OrderManager.Instance != null)

OrderManager.Instance.ShowOrder("Order: " + finalOrderToDisplay);


HideUIElements();

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



void HideUIElements()

{

if (dialogueCanvas != null)

dialogueCanvas.SetActive(false);



if (dialogueRunner != null)

{

CanvasGroup optionsGroup = dialogueRunner.GetComponentInChildren<CanvasGroup>(true);

if (optionsGroup != null)

{

optionsGroup.gameObject.SetActive(false);

}

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



if (!isWaiting && !hasArrivedAtCounter && !hasBeenServed && agent != null && counterTarget != null)

{

if (agent.remainingDistance <= agent.stoppingDistance + 0.1f && !agent.pathPending)

{

hasArrivedAtCounter = true;

agent.isStopped = true;

agent.ResetPath();

agent.velocity = Vector3.zero;



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



public void FinishOrderAndLeave()

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



public void CallToCounter()

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

{

r.enabled = isVisible;

}



if (interactionBubble != null)

interactionBubble.SetActive(isVisible);

}

}