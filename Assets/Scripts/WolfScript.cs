using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WolfTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarController carController;
    [SerializeField] private FollowCar followCar;
    [SerializeField] private DrivingSceneManager drivingSceneManager;
    [SerializeField] private GameObject wolfObject;
    [SerializeField] private Transform wolfEndPoint;
    [SerializeField] private BreathingSequence breathingSequence;
    [SerializeField] private Transform npcTransform;

    [Header("Wolf Settings")]
    [SerializeField] private float wolfSpeed = 8f;
    private Animator wolfAnimator;
    [Header("Audio")]
[SerializeField] private AudioSource jumpscareAudio;
[SerializeField] private AudioClip jumpscareClip;
[SerializeField] private AudioSource heartbeatAudio;
[SerializeField] private AudioClip heartbeatClip;

    [Header("NPC Teleport")]
    [SerializeField] private Vector3 npcOffsetFromCar = new Vector3(-1.5f, 0f, 0f);

    [Header("QTE")]
    [SerializeField] private GameObject qtePanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float qteDuration = 10f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player") && !other.CompareTag("Car")) return;

        hasTriggered = true;
        StartCoroutine(WolfSequence());
    }

    private IEnumerator WolfSequence()
    {
        // Stop car and follow car
        if (carController != null) carController.StopCar();
        if (followCar != null) followCar.StopFollowing();

       if (carController != null) carController.StopCar();
if (followCar != null) followCar.StopFollowing();

// Swap engine audio for heartbeat
if (heartbeatAudio != null && heartbeatClip != null)
{
    heartbeatAudio.clip = heartbeatClip;
    heartbeatAudio.loop = true;
    heartbeatAudio.Play();
}
        // Wolf is already placed in the middle of the road in the scene
        // Make sure it's active and visible
if (wolfObject != null)
{
    wolfObject.SetActive(true);
    wolfAnimator = wolfObject.GetComponentInChildren<Animator>();
}

        // QTE — wolf stands still while player decides
        if (qtePanel != null) qtePanel.SetActive(true);
        if (promptText != null) promptText.text = "PRESS SPACE";

        float timeLeft = qteDuration;
        bool succeeded = false;

        while (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            if (countdownText != null)
                countdownText.text = Mathf.CeilToInt(timeLeft).ToString();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                succeeded = true;
                break;
            }

            yield return null;
        }

        if (qtePanel != null) qtePanel.SetActive(false);

        if (!succeeded)
        {
            Debug.Log("[WolfTrigger] QTE failed — triggering crash ending");
            if (wolfObject != null) wolfObject.SetActive(false);
            if (heartbeatAudio != null) heartbeatAudio.Stop();
            if (drivingSceneManager != null) drivingSceneManager.TriggerCrashEnding();
            yield break;
        }

        // Success — wolf now walks off the road
        Debug.Log("[WolfTrigger] QTE succeeded — wolf clearing road");
        if (wolfAnimator != null) wolfAnimator.SetBool("isWalking", true);
        while (wolfObject != null &&
               Vector3.Distance(wolfObject.transform.position, wolfEndPoint.position) > 1f)
        {
            wolfObject.transform.position = Vector3.MoveTowards(
                wolfObject.transform.position,
                wolfEndPoint.position,
                wolfSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (wolfObject != null) wolfObject.SetActive(false);
        Debug.Log("[WolfTrigger] Wolf finished — teleporting NPC");

      // Calculate target position and pass to BreathingSequence
// NPC will teleport later while screen is black
if (npcTransform != null && carController != null)
{
    Vector3 npcPos = carController.transform.position
        + carController.transform.right * npcOffsetFromCar.x
        + carController.transform.up * npcOffsetFromCar.y
        + carController.transform.forward * npcOffsetFromCar.z;

    breathingSequence.SetNPCTarget(npcTransform, npcPos, carController.transform);
    breathingSequence.SetHeartbeat(heartbeatAudio);
}

yield return new WaitForSeconds(0.5f);

if (breathingSequence != null)
    breathingSequence.Begin();
    }
}