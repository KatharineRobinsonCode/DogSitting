using UnityEngine;

public class PizzaBoxTrigger : MonoBehaviour
{
    public static bool returningFromMiniGame = false;
    public static Vector3 savedPlayerPosition;
    public static Quaternion savedPlayerRotation;

    [SerializeField] private GameObject pizzaBox;
    [SerializeField] private AudioSource doorbellAudio;
    [SerializeField] private AudioClip doorbellClip;
private void Awake()
{
    Debug.Log("[PizzaBoxTrigger] Awake called, isReturningFromMiniGame: " + HouseSceneState.isReturningFromMiniGame);
}
    private void Start()
    {
    if (pizzaBox != null)
        pizzaBox.SetActive(false);

    if (HouseSceneState.isReturningFromMiniGame)
    {
        // Skip intro
        IntroSequence intro = FindFirstObjectByType<IntroSequence>();
        if (intro != null) intro.ForceSkip();

        // Restore player position
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = HouseSceneState.playerPosition;
            player.transform.rotation = HouseSceneState.playerRotation;
            if (cc != null) cc.enabled = true;
        }

        // Restore task
        if (TaskManager.Instance != null && !string.IsNullOrEmpty(HouseSceneState.savedTask))
            TaskManager.Instance.ShowTask(HouseSceneState.savedTask);

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
          HouseSceneState.Clear();
    }
}