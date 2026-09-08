using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BeerMatStackGame : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private RectTransform stackArea;       // where mats are shown
    [SerializeField] private GameObject matPrefabUI;        // UI mat prefab (Image)
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI tierText;

    [Header("Settings")]
    [SerializeField] private float matSlideSpeed = 150f;    // pixels per second
    [SerializeField] private float matWidth = 200f;         // starting mat width
    [SerializeField] private float matHeight = 40f;
    [SerializeField] private float matSpacing = 50f;        // vertical gap between tiers
    [SerializeField] private int totalTiers = 3;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip placeSoundClip;
    [SerializeField] private AudioClip perfectClip;
    [SerializeField] private AudioClip failClip;

    private bool isPlaying = false;
    private bool gameCompleted = false;

    public static int LastScore = 0;

    private void Start()
    {
        if (gamePanel != null) gamePanel.SetActive(false);
    }

    public string GetInteractionPrompt()
    {
        if (gameCompleted) return "";
        if (!TaskManager.Instance.IsCurrentTask("Play a game with the beer mats")) return "";
        return "Press E to start stacking";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isPlaying || gameCompleted) return;
        if (!TaskManager.Instance.IsCurrentTask("Play a game with the beer mats")) return;
        StartCoroutine(PlayGame());
    }

    private IEnumerator PlayGame()
    {
         Debug.Log($"[StackGame] PlayGame started — gamePanel null: {gamePanel == null} stackArea null: {stackArea == null} matPrefabUI null: {matPrefabUI == null}");
        isPlaying = true;

        if (gamePanel != null) gamePanel.SetActive(true);
        if (PauseManager.Instance != null) PauseManager.Instance.ShowCursorPublic();

        // Clear any existing mats
        foreach (Transform child in stackArea)
            Destroy(child.gameObject);

        float currentMatWidth = matWidth;
        float previousMatX = 0f;
        float previousMatWidth = matWidth;
        int tiersCompleted = 0;
        bool gameOver = false;

        // Place the base mat (stationary, full width)
        GameObject baseMat = Instantiate(matPrefabUI, stackArea);
        RectTransform baseRT = baseMat.GetComponent<RectTransform>();
        baseRT.sizeDelta = new Vector2(matWidth, matHeight);
        baseRT.anchoredPosition = new Vector2(0f, 0f);
        previousMatX = 0f;
        previousMatWidth = matWidth;

        if (tierText != null) tierText.text = $"Tier: 0/{totalTiers}";
        if (instructionText != null) instructionText.text = "Press SPACE to drop the mat!";
        if (resultText != null) resultText.text = "";

        // Play each tier
        for (int tier = 1; tier <= totalTiers && !gameOver; tier++)
        {
            // Sliding mat
            GameObject slidingMat = Instantiate(matPrefabUI, stackArea);
            RectTransform slidingRT = slidingMat.GetComponent<RectTransform>();
            slidingRT.sizeDelta = new Vector2(currentMatWidth, matHeight);

            float yPos = tier * (matHeight + matSpacing);
            float xPos = -stackArea.rect.width / 2f;
            float direction = 1f;
            slidingRT.anchoredPosition = new Vector2(xPos, yPos);

            Debug.Log($"[StackGame] Tier {tier} — sliding mat created at x:{xPos:F1} y:{yPos:F1} width:{currentMatWidth:F1}");

            bool dropped = false;

            // Slide until dropped
            while (!dropped)
            {
                xPos += direction * matSlideSpeed * Time.deltaTime;

                // Bounce off edges
                float halfArea = stackArea.rect.width / 2f;
                if (xPos > halfArea) { xPos = halfArea; direction = -1f; }
                if (xPos < -halfArea) { xPos = -halfArea; direction = 1f; }

                slidingRT.anchoredPosition = new Vector2(xPos, yPos);

                if (Input.GetKeyDown(KeyCode.Space))
                    dropped = true;

                yield return null;
            }

            // Calculate overlap
            float slidingLeft = xPos - currentMatWidth / 2f;
            float slidingRight = xPos + currentMatWidth / 2f;
            float previousLeft = previousMatX - previousMatWidth / 2f;
            float previousRight = previousMatX + previousMatWidth / 2f;

            float overlapLeft = Mathf.Max(slidingLeft, previousLeft);
            float overlapRight = Mathf.Min(slidingRight, previousRight);
            float overlapWidth = overlapRight - overlapLeft;

            if (overlapWidth <= 0f)
            {
                // Missed completely
                if (audioSource != null && failClip != null)
                    audioSource.PlayOneShot(failClip);
                if (resultText != null) resultText.text = "Missed! Game over.";
                Destroy(slidingMat);
                gameOver = true;
                break;
            }

            // Trim mat to overlap
            float newX = (overlapLeft + overlapRight) / 2f;
            slidingRT.sizeDelta = new Vector2(overlapWidth, matHeight);
            slidingRT.anchoredPosition = new Vector2(newX, yPos);

            bool isPerfect = Mathf.Abs(overlapWidth - previousMatWidth) < 5f;

            if (audioSource != null)
            {
                if (isPerfect && perfectClip != null)
                    audioSource.PlayOneShot(perfectClip);
                else if (placeSoundClip != null)
                    audioSource.PlayOneShot(placeSoundClip);
            }

            if (resultText != null)
                resultText.text = isPerfect ? "Perfect!" : "Good!";

            previousMatX = newX;
            previousMatWidth = overlapWidth;
            currentMatWidth = overlapWidth;
            tiersCompleted++;

            if (tierText != null) tierText.text = $"Tier: {tiersCompleted}/{totalTiers}";

            // Speed up each tier
            matSlideSpeed += 30f;

            yield return new WaitForSeconds(0.3f);
        }

        // Result
        LastScore = tiersCompleted;
        StoryFlags.Instance?.SetBeerMatStackScore(tiersCompleted);

        if (!gameOver)
        {
            if (resultText != null) resultText.text = $"Amazing! {tiersCompleted} tiers!";
            gameCompleted = true;
            TaskManager.Instance?.ShowTask("Serve customers");
            CustomerQueue queue = FindFirstObjectByType<CustomerQueue>();
            queue?.ResumeQueue();
        }

        yield return new WaitForSeconds(2.5f);

        if (gamePanel != null) gamePanel.SetActive(false);
        if (PauseManager.Instance != null) PauseManager.Instance.HideCursorPublic();

        // Reset speed for replay
        matSlideSpeed -= 30f * totalTiers;
        isPlaying = false;
    }
}