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
        if (!isFollowing || agent == null || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > followDistance)
        {
            agent.isStopped = false;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, 3f, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
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
}