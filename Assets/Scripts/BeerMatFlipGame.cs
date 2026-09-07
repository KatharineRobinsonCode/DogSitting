using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BeerMatFlipGame : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Image matImage;            // the moving mat — one image does everything
    [SerializeField] private RectTransform handRect;    // the hand/catch zone image
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Settings")]
    [SerializeField] private float matStartY = -150f;   // where mat sits at rest
    [SerializeField] private float matPeakY = 150f;     // how high it goes
    [SerializeField] private float matBaseSpeed = 400f; // starting speed
    [SerializeField] private float speedIncreasePerCatch = 25f;
    [SerializeField] private float catchWindowSize = 60f;      // pixels — how big the catch zone is
    [SerializeField] private float catchWindowReduction = 5f;  // shrinks per catch

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flipClip;
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private AudioClip missClip;

    private int score = 0;
    private bool isPlaying = false;
    private float currentCatchWindow;

    public static int LastScore = 0;

    private void Start()
    {
        if (gamePanel != null) gamePanel.SetActive(false);
    }

    public string GetInteractionPrompt()
    {
        if (isPlaying) return "";
        return "Press E to flip beer mats";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isPlaying) return;
        StartCoroutine(PlayGame());
    }

    private IEnumerator PlayGame()
    {
        isPlaying = true;
        score = 0;
        currentCatchWindow = catchWindowSize;

        if (gamePanel != null) gamePanel.SetActive(true);
        if (PauseManager.Instance != null) PauseManager.Instance.ShowCursorPublic();

        if (scoreText != null) scoreText.text = "Mats: 0";
        if (resultText != null) resultText.text = "";

        bool gameOver = false;

        while (!gameOver && score < 10)
        {
            // Reset mat to start position
            if (matImage != null)
                matImage.rectTransform.anchoredPosition = new Vector2(0f, matStartY);
            matImage.transform.localScale = Vector3.one;

            if (resultText != null) resultText.text = "";
            if (instructionText != null) instructionText.text = "Press E to flip!";

            // Wait for E
            while (!Input.GetKeyDown(KeyCode.E))
                yield return null;

            if (audioSource != null && flipClip != null)
                audioSource.PlayOneShot(flipClip);

            if (instructionText != null) instructionText.text = "Press Q to catch when it lands!";

            float matY = matStartY;
            float matSpeed = matBaseSpeed + (score * speedIncreasePerCatch);
            bool rising = true;
            bool caught = false;
            bool qteComplete = false;

            // Flip on launch
            StartCoroutine(AnimateFlip());

            while (!qteComplete)
            {
                if (rising)
                {
                    matY += matSpeed * Time.deltaTime;
                    if (matY >= matPeakY)
                    {
                        matY = matPeakY;
                        rising = false;
                        StartCoroutine(AnimateFlip()); // flip at peak
                    }
                }
                else
                {
                    matY -= matSpeed * Time.deltaTime;

                    // Only allow catch on the way down
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        float handY = handRect.anchoredPosition.y;
                        float halfWindow = currentCatchWindow / 2f;
                        caught = matY >= handY - halfWindow && matY <= handY + halfWindow;
                        qteComplete = true;
                    }

                    // Missed — fell all the way back down
                    if (matY <= matStartY)
                    {
                        matY = matStartY;
                        qteComplete = true;
                        caught = false;
                    }
                }

                if (matImage != null)
                    matImage.rectTransform.anchoredPosition = new Vector2(0f, matY);

                yield return null;
            }

            if (caught)
            {
                score++;
                if (audioSource != null && catchClip != null)
                    audioSource.PlayOneShot(catchClip);
                if (scoreText != null) scoreText.text = $"Mats: {score}";
                if (resultText != null) resultText.text = "Caught it!";
                currentCatchWindow -= catchWindowReduction;
                currentCatchWindow = Mathf.Max(currentCatchWindow, 10f);
            }
            else
            {
                if (audioSource != null && missClip != null)
                    audioSource.PlayOneShot(missClip);
                if (resultText != null) resultText.text = "Dropped it!";
                gameOver = true;
            }

            yield return new WaitForSeconds(0.8f);
        }

        LastScore = score;
        StoryFlags.Instance?.SetBeerMatFlipScore(score);

        if (resultText != null)
            resultText.text = score >= 10 ? "INCREDIBLE! 10 mats!" : $"Final score: {score}!";

        yield return new WaitForSeconds(2.5f);

        if (gamePanel != null) gamePanel.SetActive(false);
        if (PauseManager.Instance != null) PauseManager.Instance.HideCursorPublic();
        isPlaying = false;
    }

    private IEnumerator AnimateFlip()
    {
        if (matImage == null) yield break;

        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scaleY = Mathf.Abs(Mathf.Cos(t * Mathf.PI));
            matImage.transform.localScale = new Vector3(1f, scaleY, 1f);
            yield return null;
        }

        matImage.transform.localScale = Vector3.one;
    }
}