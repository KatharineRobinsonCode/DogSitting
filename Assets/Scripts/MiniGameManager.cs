using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string houseSceneName = "House";

    [Header("Knock")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip knockSound;
    [SerializeField] private float minKnockTime = 10f;
    [SerializeField] private float maxKnockTime = 20f;

    [Header("Black Screen")]
    [SerializeField] private GameObject blackScreenCanvas;
    [SerializeField] private Image blackPanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private float dialogueDisplayTime = 3f;

    private float knockTimer;
    private bool knockFired = false;

    private void Start()
    {
        knockTimer = Random.Range(minKnockTime, maxKnockTime);

        if (blackScreenCanvas != null)
            blackScreenCanvas.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (knockFired) return;

        knockTimer -= Time.deltaTime;

        if (knockTimer <= 0f)
        {
            knockFired = true;
            StartCoroutine(KnockSequence());
        }
    }

   private IEnumerator KnockSequence()
{
    // 1. Play knock sound first
    if (audioSource != null && knockSound != null)
        audioSource.PlayOneShot(knockSound);

    yield return new WaitForSeconds(0.8f);

    // 2. Show black screen
    if (blackScreenCanvas != null)
        blackScreenCanvas.SetActive(true);

    if (blackPanel != null)
        blackPanel.color = Color.black;

    yield return new WaitForSeconds(0.5f);

    // 3. Show dialogue
    if (dialogueText != null)
    {
        Debug.Log("[MiniGameManager] Setting dialogue text");
        dialogueText.text = "Jeez, that scared me...";
        dialogueText.gameObject.SetActive(true);
    }
    else
    {
        Debug.Log("[MiniGameManager] dialogueText is NULL");
    }

    yield return new WaitForSeconds(dialogueDisplayTime);

    // 4. Lock cursor and load house scene LAST
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;

    HouseSceneState.isReturningFromMiniGame = true;
    SceneManager.LoadScene(houseSceneName);
}
}