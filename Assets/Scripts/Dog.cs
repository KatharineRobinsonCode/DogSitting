using UnityEngine;

public class Dog : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private string dogName = "the dog";
    [SerializeField] private Animator dogAnimator;
    [SerializeField] private string petAnimationName = "Pet";

    [Header("Audio")]
    [SerializeField] private AudioSource dogAudio;
    [SerializeField] private AudioClip petSound;

    private bool isPetting = false;

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
        yield return new WaitForSeconds(dogAnimator.GetCurrentAnimatorStateInfo(0).length);
        isPetting = false;
    }
}