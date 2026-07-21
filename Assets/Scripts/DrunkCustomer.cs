using UnityEngine;
using UnityEngine.AI;
using Yarn.Unity;
using System.Collections;

public class DrunkCustomer : NpcCustomer
{
    [Header("Drunk Settings")]
    public string serveCommandName = "CompleteOrderConversation_Customer6_Serve";
    public string kickCommandName = "CompleteOrderConversation_Customer6_Kick";

    private bool wasKickedOut = false;
     private bool isFacingPlayer = false;
    private Animator drunkAnim;
    private NavMeshAgent drunkAgent;

    [Header("Look At Player")]
[SerializeField] private Transform player;
[SerializeField] private float turnSpeed = 7f;
private bool isFacingPlayer = false;

    protected override void Start()
    {
        base.Start();

        drunkAnim = GetComponentInChildren<Animator>();
        drunkAgent = GetComponent<NavMeshAgent>();

        // Start seated
        if (drunkAnim != null)
            drunkAnim.SetBool("isStanding", false);

        // Disable agent while seated
        if (drunkAgent != null)
            drunkAgent.enabled = false;
    }
protected override void Update()
{
    if (isWaiting) return;
    base.Update();

    if (isFacingPlayer && player != null)
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                Time.deltaTime * turnSpeed
            );
        }
    }
}

protected override void RegisterAdditionalYarnCommands(DialogueRunner runner)
{
    base.RegisterAdditionalYarnCommands(runner);
    runner.AddCommandHandler(serveCommandName, CompleteOrderConversationServe);
    runner.AddCommandHandler(kickCommandName, CompleteOrderConversationKick);
}
public override void CallToCounter()
{
    Debug.Log("[DrunkCustomer] CallToCounter called");
    isWaiting = false;
    StartCoroutine(CallToCounterRoutine());
}

private IEnumerator CallToCounterRoutine()
{
    if (agent != null)
    {
        agent.enabled = true;
        yield return null; // give Unity a frame to register the agent on NavMesh
        agent.Warp(transform.position);
        agent.speed = walkSpeed;
        agent.SetDestination(counterTarget.position);
    }

    if (anim != null)
        anim.SetBool("isStanding", true);
}

    public void CompleteOrderConversationServe()
    {
        if (OrderManager.Instance != null)
            OrderManager.Instance.ShowOrder("Order: " + finalOrderToDisplay);

        if (TaskManager.Instance != null)
            TaskManager.Instance.ShowTask("Make " + finalOrderToDisplay);

        HideUIElements();
    }
public void CompleteOrderConversationKick()
{
    wasKickedOut = true;
    HideUIElements();

    // Notify order manager
    if (OrderManager.Instance != null)
    {
        OrderManager.Instance.CustomerLeft();
        OrderManager.Instance.HideOrder();
    }

    // Notify queue since we're skipping the register
    CustomerQueue queue = FindFirstObjectByType<CustomerQueue>();
    if (queue != null)
        queue.CustomerLeft(this);

    FinishOrderAndLeave();
}

    // Override arrival to switch to drunk idle instead of normal idle
    protected override void OnArrivedAtCounter()
    {
        base.OnArrivedAtCounter();

        if (drunkAnim != null)
            drunkAnim.SetBool("isStanding", true);
    }
    public override void FinishOrderAndLeave()
{
    hasBeenServed = true;
    hasArrivedAtCounter = false;
    isLeaving = true;

    if (agent != null)
    {
        agent.enabled = true;
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.SetDestination(exitPoint.position);
    }
}
}