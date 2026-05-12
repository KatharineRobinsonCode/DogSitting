using UnityEngine;

public class PizzaBoxTrigger : MonoBehaviour
{
    [SerializeField] private GameObject pizzaBox;
    [SerializeField] private AudioSource doorbellAudio;
    [SerializeField] private AudioClip doorbellClip;

    private void Start()
    {
        // Hide pizza box initially
        if (pizzaBox != null)
            pizzaBox.SetActive(false);

        // If returning from mini game, show pizza box
        if (TaskManager.Instance != null &&
            TaskManager.Instance.IsCurrentTask("Wait for the Pizza"))
        {
            StartCoroutine(PizzaArrival());
        }
    }

    private System.Collections.IEnumerator PizzaArrival()
    {
        yield return new WaitForSeconds(1f);

        if (doorbellAudio != null && doorbellClip != null)
            doorbellAudio.PlayOneShot(doorbellClip);

        yield return new WaitForSeconds(1f);

        if (pizzaBox != null)
            pizzaBox.SetActive(true);

        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask();
    }
}