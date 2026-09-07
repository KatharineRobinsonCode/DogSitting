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

    if (scoreText != null) scoreText.text = "Mats: 0";
    if (resultText != null) resultText.text = "";

    // Green zone is a fixed band in the middle-upper area of the panel
    float panelHeight = 400f;           // height of your panel in pixels
    float greenZoneCentre = 280f;       // Y position of green zone from bottom
    float matStartY = -panelHeight / 2f + 30f;  // mat starts near bottom
    float matPeakY = panelHeight / 2f - 30f;    // mat goes near top

    UpdateGreenZoneVisual(greenZoneCentre, panelHeight);

    bool gameOver = false;

    while (!gameOver && score < 10)
    {
        if (catchBar != null)
            catchBar.rectTransform.anchoredPosition = new Vector2(0f, matStartY);

        if (resultText != null) resultText.text = "";
        if (instructionText != null) instructionText.text = "Press E to flip!";

        // Wait for E
        while (!Input.GetKeyDown(KeyCode.E))
            yield return null;

        if (audioSource != null && flipClip != null)
            audioSource.PlayOneShot(flipClip);

        if (instructionText != null) instructionText.text = "Press Q to catch when it lands!";
        float matY = matStartY;
        float matSpeed = 400f + (score * 20f);  // gets faster each mat
        bool going = true;                       // true = rising, false = falling
        bool caught = false;
        bool qteComplete = false;

        // Flip animation runs once as it launches
        StartCoroutine(AnimateFlip());

        while (!qteComplete)
        {
            // Move mat up then down
            if (going)
            {
                matY += matSpeed * Time.deltaTime;
                if (matY >= matPeakY)
                {
                    matY = matPeakY;
                    going = false;
                    StartCoroutine(AnimateFlip()); // flip again at peak
                }
            }
            else
            {
                matY -= matSpeed * Time.deltaTime;
                if (matY <= matStartY)
                {
                    matY = matStartY;
                    qteComplete = true;
                    caught = false; // fell all the way — missed
                }
            }

            // Move the mat image
            if (catchBar != null)
                catchBar.rectTransform.anchoredPosition = new Vector2(0f, matY);

         // Check Q press — only on the way DOWN
if (!going && Input.GetKeyDown(KeyCode.Q))
{
    float matPosY = catchBar.rectTransform.anchoredPosition.y;
    float greenY = greenZoneRect.anchoredPosition.y;
    float halfGreen = greenZoneRect.sizeDelta.y / 2f;
    caught = matPosY >= greenY - halfGreen && matPosY <= greenY + halfGreen;
    qteComplete = true;
}

            yield return null;
        }

        if (caught)
        {
            score++;
            if (audioSource != null && catchClip != null)
                audioSource.PlayOneShot(catchClip);
            if (scoreText != null) scoreText.text = $"Mats: {score}";
            if (resultText != null) resultText.text = "Caught it!";
            currentGreenZoneSize -= greenZoneReduction * panelHeight;
            currentGreenZoneSize = Mathf.Max(currentGreenZoneSize, greenZoneMin * panelHeight);
            UpdateGreenZoneVisual(greenZoneCentre, panelHeight);
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

private void UpdateGreenZoneVisual(float centre, float panelHeight)
{
    if (greenZoneRect == null) return;
    greenZoneRect.anchoredPosition = new Vector2(0f, centre - panelHeight / 2f);
    greenZoneRect.sizeDelta = new Vector2(greenZoneRect.sizeDelta.x, currentGreenZoneSize);
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
        float scaleY = Mathf.Abs(Mathf.Cos(t * Mathf.PI));
        matStackImage.transform.localScale = new Vector3(1f, scaleY, 1f);
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