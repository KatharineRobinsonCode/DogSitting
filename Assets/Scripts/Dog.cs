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

    private bool isPetting = false;
    private bool isFollowing = false;
    private bool isMovingToTarget = false;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

            if (InventoryManager.Instance != null)
        InventoryManager.Instance.SetBrinkley(this);
    }

    private void Update()
    {
        // Case 1: Moving to a specific target (bowl/closet)
        if (isMovingToTarget && agent != null && agent.enabled)
        {
            if (dogAnimator != null)
                dogAnimator.SetBool("IsWalking", true);

            // Check if arrived
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isMovingToTarget = false;
                agent.isStopped = true;

                if (dogAnimator != null)
                    dogAnimator.SetBool("IsWalking", false);

                // Face the look target if assigned
                if (lookTarget != null)
                    StartCoroutine(FaceTarget(lookTarget));
            }
            return;
        }

        // Case 2: Not following — just idle
        if (!isFollowing || agent == null || player == null)
        {
            if (dogAnimator != null)
                dogAnimator.SetBool("IsWalking", false);
            return;
        }

        // Case 3: Following player
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > followDistance)
        {
            agent.isStopped = false;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 3f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);

            if (dogAnimator != null)
                dogAnimator.SetBool("IsWalking", true);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();

            if (dogAnimator != null)
                dogAnimator.SetBool("IsWalking", false);
        }
    }

    private System.Collections.IEnumerator FaceTarget(Transform target)
    {
        float elapsed = 0f;
        float duration = 0.5f;
        Quaternion startRotation = transform.rotation;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction == Vector3.zero) yield break;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
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
        isFollowing = false;
        isMovingToTarget = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public void GoToPosition(Transform target)
    {
            Debug.Log("[Dog] GoToPosition called, target: " + target.name);
        if (agent == null || target == null) return;

        isFollowing = false;
        isMovingToTarget = true;

        agent.enabled = true;
        agent.isStopped = false;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(target.position, out hit, 3f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }
    public void ComeToPlayer()
{
    if (agent == null || player == null) return;

    isFollowing = false;
    isMovingToTarget = true;

    agent.enabled = true;
    agent.isStopped = false;

    NavMeshHit hit;
    if (NavMesh.SamplePosition(player.position, out hit, 3f, NavMesh.AllAreas))
        agent.SetDestination(hit.position);

    Debug.Log("[Dog] ComeToPlayer called — heading to player");
}
    
}