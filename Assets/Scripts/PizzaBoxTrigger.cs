using UnityEngine;

public class PizzaBoxTrigger : MonoBehaviour
{
    public static bool returningFromMiniGame = false;
    public static Vector3 savedPlayerPosition;
    public static Quaternion savedPlayerRotation;

    [SerializeField] private GameObject pizzaBox;
    [SerializeField] private AudioSource doorbellAudio;
    [SerializeField] private AudioClip doorbellClip;

    private void Start()
    {
        if (pizzaBox != null)
            pizzaBox.SetActive(false);

        if (returningFromMiniGame)
        {
            returningFromMiniGame = false;

            // Skip intro
            IntroSequence intro = FindFirstObjectByType<IntroSequence>();
            if (intro != null)
                intro.ForceSkip();

            // Restore player position
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // CharacterController must be disabled before teleporting
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                player.transform.position = savedPlayerPosition;
                player.transform.rotation = savedPlayerRotation;

                if (cc != null) cc.enabled = true;
            }

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