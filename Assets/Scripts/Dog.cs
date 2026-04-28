using UnityEngine;
using UnityEngine.AI;

public class Dog : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string dogName = "the dog";
    [SerializeField] private Animator dogAnimator;
    [SerializeField] private string petAnimationName = "Pet";

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
    Debug.Log($"[Dog] dist: {dist}, onNavMesh: {agent.isOnNavMesh}, speed: {agent.speed}, pathStatus: {agent.pathStatus}, velocity: {agent.velocity}");
    if (dist > followDistance) agent.SetDestination(player.position);
    else agent.ResetPath();
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
    yield return new WaitForSeconds(0.1f); // small delay to let animator transition
    float animLength = dogAnimator.GetCurrentAnimatorStateInfo(0).length;
    Debug.Log($"[Dog] Waiting for animation: {animLength}s");
    yield return new WaitForSeconds(animLength);
    
    Debug.Log($"[Dog] Animation done. Agent on NavMesh: {agent.isOnNavMesh}");
    isPetting = false;
    if (!isFollowing)
    {
        isFollowing = true;
        if (agent != null) agent.enabled = true;
        Debug.Log("[Dog] Now following");
    }
}
}