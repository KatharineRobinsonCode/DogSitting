using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BeerMatFlipGame : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Image matStackImage;
    [SerializeField] private Image catchBar;
    [SerializeField] private RectTransform greenZoneRect;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Mat Sprites")]
    [SerializeField] private Sprite[] matStackSprites;

    [Header("Settings")]
    [SerializeField] private float barRiseSpeed = 0.5f;
    [SerializeField] private float greenZoneSize = 0.25f;
    [SerializeField] private float greenZoneReduction = 0.02f;
    [SerializeField] private float greenZoneMin = 0.05f;
    [SerializeField] private float barHeight = 300f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flipClip;
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private AudioClip missClip;

    private int score = 0;
    private bool isPlaying = false;
    private float currentGreenZoneSize;
    private float greenZonePosition = 0.65f;

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
        currentGreenZoneSize = greenZoneSize;

        if (gamePanel != null) gamePanel.SetActive(true);
        if (PauseManager.Instance != null) PauseManager.Instance.ShowCursorPublic();

        UpdateMatStack();
        UpdateGreenZone();

        if (scoreText != null) scoreText.text = "Mats: 0";
        if (resultText != null) resultText.text = "";

        bool gameOver = false;

        while (!gameOver && score < 10)
        {
            if (catchBar != null) catchBar.fillAmount = 0f;
            if (resultText != null) resultText.text = "";
            if (instructionText != null) instructionText.text = "Press E to flip!";

            // Wait for E to flip
            while (!Input.GetKeyDown(KeyCode.E))
                yield return null;

            // Flip sound
            if (audioSource != null && flipClip != null)
                audioSource.PlayOneShot(flipClip);

            if (instructionText != null) instructionText.text = "Press Q to catch!";

            float barValue = 0f;
            bool qteComplete = false;
            bool caught = false;

            // Rise
            while (barValue < 1f && !qteComplete)
            {
                barValue += barRiseSpeed * Time.deltaTime;
                barValue = Mathf.Clamp01(barValue);
                if (catchBar != null) catchBar.fillAmount = barValue;

                if (Input.GetKeyDown(KeyCode.Q))
                {
                    caught = CheckGreenZone(barValue);
                    qteComplete = true;
                }

                yield return null;
            }

            // Fall back down if not caught on the way up
            if (!qteComplete)
            {
                while (barValue > 0f && !qteComplete)
                {
                    barValue -= barRiseSpeed * Time.deltaTime;
                    barValue = Mathf.Clamp(barValue, 0f, 1f);
                    if (catchBar != null) catchBar.fillAmount = barValue;

                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        caught = CheckGreenZone(barValue);
                        qteComplete = true;
                    }

                    yield return null;
                }
            }

            // Fell all the way down without catching
            if (!qteComplete) caught = false;

            // Animate flip
            yield return StartCoroutine(AnimateFlip());

            if (caught)
            {
                score++;
                if (audioSource != null && catchClip != null)
                    audioSource.PlayOneShot(catchClip);
                if (scoreText != null) scoreText.text = $"Mats: {score}";
                if (resultText != null) resultText.text = score == 10 ? "PERFECT!" : "Nice catch!";

                // Shrink green zone for next mat
                currentGreenZoneSize -= greenZoneReduction;
                currentGreenZoneSize = Mathf.Max(currentGreenZoneSize, greenZoneMin);
                UpdateMatStack();
                UpdateGreenZone();
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

        // Final result
        LastScore = score;
        StoryFlags.Instance?.SetBeerMatFlipScore(score);

        if (resultText != null)
            resultText.text = score >= 10
                ? "INCREDIBLE! 10 mats!"
                : $"Final score: {score} mat{(score == 1 ? "" : "s")}!";

        yield return new WaitForSeconds(2.5f);

        if (gamePanel != null) gamePanel.SetActive(false);
        if (PauseManager.Instance != null) PauseManager.Instance.HideCursorPublic();
        isPlaying = false;
    }

    private bool CheckGreenZone(float barValue)
    {
        float greenMin = greenZonePosition - currentGreenZoneSize / 2f;
        float greenMax = greenZonePosition + currentGreenZoneSize / 2f;
        return barValue >= greenMin && barValue <= greenMax;
    }

    private IEnumerator AnimateFlip()
    {
        if (matStackImage == null) yield break;

        float elapsed = 0f;
        float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scaleX = Mathf.Abs(Mathf.Cos(t * Mathf.PI));
            matStackImage.transform.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        matStackImage.transform.localScale = Vector3.one;
    }

    private void UpdateMatStack()
    {
        if (matStackImage == null || matStackSprites == null || matStackSprites.Length == 0) return;
        int index = Mathf.Clamp(score, 0, matStackSprites.Length - 1);
        matStackImage.sprite = matStackSprites[index];
    }

    private void UpdateGreenZone()
    {
        if (greenZoneRect == null) return;
        greenZoneRect.sizeDelta = new Vector2(
            greenZoneRect.sizeDelta.x,
            currentGreenZoneSize * barHeight
        );
        greenZoneRect.anchoredPosition = new Vector2(
            greenZoneRect.anchoredPosition.x,
            greenZonePosition * barHeight - barHeight / 2f
        );
    }
}