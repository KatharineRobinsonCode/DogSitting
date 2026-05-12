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
        Debug.Log("[MiniGameManager] About to load house — isReturningFromMiniGame: " + HouseSceneState.isReturningFromMiniGame);
HouseSceneState.isReturningFromMiniGame = true;
Debug.Log("[MiniGameManager] After setting — isReturningFromMiniGame: " + HouseSceneState.isReturningFromMiniGame);
SceneManager.LoadScene(houseSceneName);

        if (audioSource != null && knockSound != null)
            audioSource.PlayOneShot(knockSound);

        yield return new WaitForSeconds(0.8f);

        if (blackScreenCanvas != null)
            blackScreenCanvas.SetActive(true);

        if (blackPanel != null)
            blackPanel.color = Color.black;

        yield return new WaitForSeconds(0.5f);

        if (dialogueText != null)
        {
            dialogueText.text = "Jeez, that scared me...";
            dialogueText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(dialogueDisplayTime);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        HouseSceneState.isReturningFromMiniGame = true;
        SceneManager.LoadScene(houseSceneName);
    }
}