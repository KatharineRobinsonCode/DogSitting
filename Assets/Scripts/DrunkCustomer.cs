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
    private Animator drunkAnim;
    private NavMeshAgent drunkAgent;

    protected override void Start()
    {
        base.Start();

        drunkAnim = GetComponentInChildren<Animator>();
        drunkAgent = GetComponent<NavMeshAgent>();

        // Start seated
        if (drunkAnim != null)
            drunkAnim.SetBool("IsStanding", false);

        // Disable agent while seated
        if (drunkAgent != null)
            drunkAgent.enabled = false;
    }
protected override void Update()
{
    if (isWaiting) return; // Don't do anything while sitting
    base.Update();
}
    // Register extra commands on top of base ones
    protected override void RegisterAdditionalYarnCommands(DialogueRunner runner)
    {
        base.RegisterAdditionalYarnCommands(runner);
        runner.AddCommandHandler(serveCommandName, CompleteOrderConversationServe);
        runner.AddCommandHandler(kickCommandName, CompleteOrderConversationKick);
    }

    public new void CallToCounter()
    {
        // Enable agent and start walking
        if (drunkAgent != null)
        {
            drunkAgent.enabled = true;
            drunkAgent.Warp(transform.position);
            drunkAgent.speed = walkSpeed;
            drunkAgent.SetDestination(counterTarget.position);
        }

        if (drunkAnim != null)
            drunkAnim.SetBool("IsStanding", true);

        isWaiting = false;
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

        if (TaskManager.Instance != null)
            TaskManager.Instance.ShowTask("Serve customers");

        FinishOrderAndLeave();
    }

    // Override arrival to switch to drunk idle instead of normal idle
    protected override void OnArrivedAtCounter()
    {
        base.OnArrivedAtCounter();

        if (drunkAnim != null)
            drunkAnim.SetBool("IsStanding", true);
    }
}