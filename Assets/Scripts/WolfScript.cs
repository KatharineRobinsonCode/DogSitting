using UnityEngine;
using System.Collections;

public class WolfTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController carController;
    [SerializeField] private FollowCar followCar;
    [SerializeField] private GameObject wolfObject;
    [SerializeField] private Transform wolfEndPoint;
    [SerializeField] private BreathingSequence breathingSequence;
    [SerializeField] private Transform npcTransform;

    [Header("Wolf Settings")]
    [SerializeField] private float wolfSpeed = 8f;
    [SerializeField] private string wolfRunTrigger = "Run";

    [Header("Audio")]
    [SerializeField] private AudioSource jumpscareAudio;
    [SerializeField] private AudioClip jumpscareClip;

    [Header("NPC Teleport")]
    [SerializeField] private Vector3 npcOffsetFromCar = new Vector3(-1.5f, 0f, 0f);

    private bool hasTriggered = false;
    private bool wolfRunning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player") && !other.CompareTag("Car")) return;

        hasTriggered = true;
        StartCoroutine(WolfSequence());
    }

    private IEnumerator WolfSequence()
   {
    if (carController != null)
    {
        carController.StopCar();
        carController.DisableControls(); // ← add this
    }
        if (followCar != null) followCar.StopFollowing();

        // Play jumpscare
        if (jumpscareAudio != null && jumpscareClip != null)
            jumpscareAudio.PlayOneShot(jumpscareClip);

        // Activate wolf and start running
        if (wolfObject != null)
        {
            wolfObject.SetActive(true);
            Animator wolfAnim = wolfObject.GetComponentInChildren<Animator>();
            if (wolfAnim != null)
                wolfAnim.SetTrigger(wolfRunTrigger);
            wolfRunning = true;
        }

        // Wait for wolf to reach end point
        while (wolfRunning && wolfObject != null &&
               Vector3.Distance(wolfObject.transform.position, wolfEndPoint.position) > 1f)
        {
            wolfObject.transform.position = Vector3.MoveTowards(
                wolfObject.transform.position,
                wolfEndPoint.position,
                wolfSpeed * Time.deltaTime
            );
            yield return null;
        }

        wolfRunning = false;
        if (wolfObject != null) wolfObject.SetActive(false);

        // Teleport NPC to car window
        if (npcTransform != null && carController != null)
        {
            Vector3 npcPos = carController.transform.position
                + carController.transform.right * npcOffsetFromCar.x
                + carController.transform.up * npcOffsetFromCar.y
                + carController.transform.forward * npcOffsetFromCar.z;
            npcTransform.position = npcPos;
            npcTransform.LookAt(carController.transform);
        }

        // Small beat before breathing sequence
        yield return new WaitForSeconds(0.5f);

        // Start breathing sequence
        if (breathingSequence != null)
            breathingSequence.Begin();
    }
}