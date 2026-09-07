using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BeerMatFlipGame : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Image matStackImage;        // shows the stack of mats
    [SerializeField] private Image catchBar;             // the rising bar
    [SerializeField] private Image greenZone;            // the catch zone
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Mat Sprites")]
    [SerializeField] private Sprite[] matStackSprites;   // 1 mat, 2 mats etc up to 10

    [Header("Settings")]
    [SerializeField] private float barRiseSpeed = 0.4f;
    [SerializeField] private float catchWindowStart = 0.3f;   // green zone size starts here
    [SerializeField] private float catchWindowReduction = 0.03f; // shrinks per mat
    [SerializeField] private float flipDuration = 0.3f;        // time to animate flip

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flipClip;
    [SerializeField] private AudioClip catchClip;
    [SerializeField] private AudioClip missClip;

    private int score = 0;
    private bool isPlaying = false;
    private bool isFlipping = false;
    private float barValue = 0f;
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
        currentCatchWindow = catchWindowStart;
        barValue = 0f;

        if (gamePanel != null) gamePanel.SetActive(true);
        if (PauseManager.Instance != null) PauseManager.Instance.ShowCursorPublic();

        UpdateMatStack();
        UpdateGreenZone();

        if (instructionText != null)
            instructionText.text = "Hold Q to build height — release to flip!";
        if (resultText != null) resultText.text = "";
        if (scoreText != null) scoreText.text = "Mats: 0";

        bool gameOver = false;

        while (!gameOver && score < 10)
        {
            barValue = 0f;
            bool caught = false;
            bool flipping = false;

            // Wait for Q hold
            while (!flipping)
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    barValue += barRiseSpeed * Time.deltaTime;
                    barValue = Mathf.Clamp01(barValue);

                    if (catchBar != null) catchBar.fillAmount = barValue;

                    // Auto fail if bar maxes out
                    if (barValue >= 1f)
                    {
                        flipping = true;
                        caught = false;
                    }
                }
                else if (Input.GetKeyUp(KeyCode.Q) && barValue > 0f)
                {
                    flipping = true;
                    // Check if in green zone
                    float greenMin = greenZone.rectTransform.anchoredPosition.y / 300f;
                    float greenMax = greenMin + currentCatchWindow;
                    caught = barValue >= greenMin && barValue <= greenMax;
                }

                yield return null;
            }

            // Animate flip
            yield return StartCoroutine(AnimateFlip(caught));

            if (caught)
            {
                score++;
                if (audioSource != null && catchClip != null)
                    audioSource.PlayOneShot(catchClip);
                if (scoreText != null) scoreText.text = $"Mats: {score}";
                if (resultText != null) resultText.text = "Nice catch!";
                currentCatchWindow -= catchWindowReduction;
                currentCatchWindow = Mathf.Max(currentCatchWindow, 0.05f);
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

            yield return new WaitForSeconds(0.5f);
        }

        // Game over
        LastScore = score;
        StoryFlags.Instance?.SetBeerMatFlipScore(score);

        if (resultText != null)
            resultText.text = score >= 10 ? "PERFECT! 10 mats!" : $"Final score: {score} mats!";

        yield return new WaitForSeconds(2f);

        if (gamePanel != null) gamePanel.SetActive(false);
        if (PauseManager.Instance != null) PauseManager.Instance.HideCursorPublic();
        isPlaying = false;
    }

    private IEnumerator AnimateFlip(bool success)
    {
        if (audioSource != null && flipClip != null)
            audioSource.PlayOneShot(flipClip);

        // Squash and stretch on X to simulate flip
        if (matStackImage != null)
        {
            float elapsed = 0f;
            while (elapsed < flipDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flipDuration;
                float scaleX = Mathf.Abs(Mathf.Cos(t * Mathf.PI));
                matStackImage.transform.localScale = new Vector3(scaleX, 1f, 1f);
                yield return null;
            }
            matStackImage.transform.localScale = Vector3.one;
        }
    }

    private void UpdateMatStack()
    {
        if (matStackImage == null || matStackSprites == null) return;
        int index = Mathf.Clamp(score, 0, matStackSprites.Length - 1);
        matStackImage.sprite = matStackSprites[index];
    }

    private void UpdateGreenZone()
    {
        if (greenZone == null) return;
        // Shrink green zone height as score increases
        RectTransform rt = greenZone.rectTransform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, currentCatchWindow * 300f);
    }
}