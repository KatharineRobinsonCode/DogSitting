using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string dogName = "the dog";
    [SerializeField] private Animator dogAnimator;

    [Header("Audio")]
    [SerializeField] private AudioSource dogAudio;
    [SerializeField] private AudioClip petSound;

    [Header("Following")]
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;

[Header("Look Target")]
[SerializeField] private Transform lookTarget;
private bool hasReachedDestination = false;

    private bool isPetting = false;
    private bool isFollowing = false;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;
    }

   private void Update()
{
    if (!isFollowing || agent == null || player == null)
    {
         // Check if Brinkley has reached his NavMesh destination
        if (agent != null && agent.enabled && !agent.isStopped && 
            !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (lookTarget != null)
            {
                // Rotate to face the look target
                Vector3 direction = lookTarget.position - transform.position;
                direction.y = 0f; // Keep rotation on Y axis only
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(
                        transform.rotation, 
                        targetRotation, 
                        Time.deltaTime * 5f
                    );
                }
            }
        }
        // Tell animator Brinkley is not walking
        if (dogAnimator != null)
            dogAnimator.SetBool("IsWalking", false);
        return;
    }

    float dist = Vector3.Distance(transform.position, player.position);
    if (dist > followDistance)
    {
        agent.isStopped = false;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(player.position, out hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        // Brinkley is moving — play walking animation
        if (dogAnimator != null)
            dogAnimator.SetBool("IsWalking", true);
    }
    else
    {
        agent.isStopped = true;
        agent.ResetPath();

        // Brinkley reached player — back to idle
        if (dogAnimator != null)
            dogAnimator.SetBool("IsWalking", false);
    }
}

    public string GetInteractionPrompt()
    {
        return isPetting ? "" : $"Press E to pet {dogName}";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isPetting) return;
        isPetting = true;

        dogAnimator.SetTrigger("Pet");

        if (dogAudio != null && petSound != null)
            dogAudio.PlayOneShot(petSound);

                // Complete "Find Brinkley" task on first pet only
    if (!isFollowing && TaskManager.Instance != null)
        TaskManager.Instance.CompleteTask();

        StartCoroutine(ResetAfterAnimation());
    }

    private System.Collections.IEnumerator ResetAfterAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        float animLength = dogAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);

        isPetting = false;

        if (!isFollowing && agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
            yield return null;

            if (agent.isOnNavMesh)
            {
                isFollowing = true;
                agent.isStopped = false;
            }
            else
            {
                agent.enabled = false;
            }
        }
    }

    public void StopFollowing()
{
        Debug.Log("[Dog] StopFollowing called");
    isFollowing = false;
    if (agent != null)
    {
        agent.isStopped = true;
        agent.ResetPath();
    }
}
public void GoToPosition(Transform target)
{
    if (agent == null || target == null) return;
    
    isFollowing = false;
    agent.enabled = true;
    agent.isStopped = false;
    
    NavMeshHit hit;
    if (NavMesh.SamplePosition(target.position, out hit, 3f, NavMesh.AllAreas))
        agent.SetDestination(hit.position);
}
}