using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BeerMatStackGame : MonoBehaviour, IInteractable
{
    public static bool IsPlaying { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private Transform stackArea;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI tierText;

    [Header("Mat Prefab")]
    [SerializeField] private GameObject matPrefabUI;
    [SerializeField] private float matWidth = 40f;
    [SerializeField] private float matHeight = 120f;
    [SerializeField] private float flatMatWidth = 160f;
    [SerializeField] private float flatMatHeight = 40f;

    [Header("Base Layout")]
    [SerializeField] private float pairGap = 80f;
    [SerializeField] private float withinPairGap = 60f;

    [Header("Target Zones")]
    [SerializeField] private float targetTolerance = 60f;
    [SerializeField] private float slideSpeed = 200f;
    [SerializeField] private float speedIncrease = 50f;

    [Header("Fall Animation")]
    [SerializeField] private float fallSpeed = 400f;
    [SerializeField] private float fallDuration = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip placeSoundClip;
    [SerializeField] private AudioClip failClip;
    [SerializeField] private AudioClip successClip;

    private bool isPlaying = false;
    private bool gameCompleted = false;
    private bool _lastRoundSuccess = false;
    private List<GameObject> spawnedMats = new List<GameObject>();

    public static int LastScore = 0;

    private void Start()
    {
        if (gamePanel != null) gamePanel.SetActive(false);
    }

    public string GetInteractionPrompt()
    {
        if (gameCompleted) return "";
        if (TaskManager.Instance == null) return "";
        if (!TaskManager.Instance.IsCurrentTask("Play a game with the beer mats")) return "";
        return "Press E to start stacking";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isPlaying || gameCompleted) return;
        if (TaskManager.Instance == null) return;
        if (!TaskManager.Instance.IsCurrentTask("Play a game with the beer mats")) return;
        StartCoroutine(PlayGame());
    }

    private IEnumerator PlayGame()
    {
        isPlaying = true;
        IsPlaying = true;
        LastScore = 0;

        if (gamePanel != null) gamePanel.SetActive(true);
        if (PauseManager.Instance != null) PauseManager.Instance.ShowCursorPublic();

        foreach (var m in spawnedMats) if (m != null) Destroy(m);
        spawnedMats.Clear();

        if (resultText != null) resultText.text = "";
        if (tierText != null) tierText.text = "Stack the mats!";
        if (instructionText != null) instructionText.text = "Press SPACE to place!";

        // Base tier — 4 static upright mats
        float halfGap = (pairGap + withinPairGap) / 2f;
        float leftPairCentre = -halfGap;
        float rightPairCentre = halfGap;

        SpawnUpright(leftPairCentre - withinPairGap / 2f, 0f);
        SpawnUpright(leftPairCentre + withinPairGap / 2f, 0f);
        SpawnUpright(rightPairCentre - withinPairGap / 2f, 0f);
        SpawnUpright(rightPairCentre + withinPairGap / 2f, 0f);

        float leftTargetX = leftPairCentre;
        float rightTargetX = rightPairCentre;
        float flatMatY = matHeight + flatMatHeight / 2f;

        yield return new WaitForSeconds(0.5f);

        // Tier 2 — left flat mat
        if (tierText != null) tierText.text = "Tier 2 — balance the flat mats";

        _lastRoundSuccess = false;
        yield return StartCoroutine(SlidingMatRound(flatMatWidth, flatMatHeight, flatMatY, leftTargetX, slideSpeed));
        if (!_lastRoundSuccess)
        {
            yield return StartCoroutine(FailSequence());
            yield break;
        }
        LastScore++;

        // Tier 2 — right flat mat
        _lastRoundSuccess = false;
        yield return StartCoroutine(SlidingMatRound(flatMatWidth, flatMatHeight, flatMatY, rightTargetX, slideSpeed + speedIncrease));
        if (!_lastRoundSuccess)
        {
            yield return StartCoroutine(FailSequence());
            yield break;
        }
        LastScore++;

        yield return new WaitForSeconds(0.3f);

        // Tier 3 — arch slides in as one unit
        if (tierText != null) tierText.text = "Tier 3 — place the arch!";

        float archTargetX = 0f;
        float archY = flatMatY + flatMatHeight;
        float archWidth = pairGap + withinPairGap * 2f;

        _lastRoundSuccess = false;
        yield return StartCoroutine(SlidingMatRound(archWidth, flatMatHeight, archY, archTargetX, slideSpeed + speedIncrease * 2f));
        if (!_lastRoundSuccess)
        {
            yield return StartCoroutine(FailSequence());
            yield break;
        }
        LastScore++;

        // Success
        if (audioSource != null && successClip != null)
            audioSource.PlayOneShot(successClip);

        StoryFlags.Instance?.SetBeerMatStackScore(LastScore);

        if (resultText != null) resultText.text = "Perfect stack! 🍺";
        if (tierText != null) tierText.text = "";

        gameCompleted = true;
        TaskManager.Instance?.ShowTask("Serve customers");
        CustomerQueue queue = FindFirstObjectByType<CustomerQueue>();
        queue?.ResumeQueue();

        yield return new WaitForSeconds(2.5f);
        EndGame();
    }

    private void SpawnUpright(float x, float baseY)
    {
        GameObject mat = Instantiate(matPrefabUI, stackArea);
        RectTransform rt = mat.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(matWidth, matHeight);
        rt.anchoredPosition = new Vector2(x, baseY + matHeight / 2f);
        spawnedMats.Add(mat);
    }

    private IEnumerator SlidingMatRound(float width, float height, float yPos, float targetX, float speed)
    {
        GameObject mat = Instantiate(matPrefabUI, stackArea);
        RectTransform rt = mat.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);

        RectTransform areaRT = stackArea.GetComponent<RectTransform>();
        float halfArea = areaRT != null ? areaRT.rect.width / 2f : 300f;

        float xPos = -halfArea;
        float direction = 1f;
        rt.anchoredPosition = new Vector2(xPos, yPos);
        spawnedMats.Add(mat);

        bool dropped = false;

        while (!dropped)
        {
            xPos += direction * speed * Time.deltaTime;

            if (xPos > halfArea) { xPos = halfArea; direction = -1f; }
            if (xPos < -halfArea) { xPos = -halfArea; direction = 1f; }

            rt.anchoredPosition = new Vector2(xPos, yPos);

            if (Input.GetKeyDown(KeyCode.Space))
                dropped = true;

            yield return null;
        }

        float distance = Mathf.Abs(xPos - targetX);
        bool success = distance <= targetTolerance;

        if (success)
        {
            rt.anchoredPosition = new Vector2(targetX, yPos);
            if (audioSource != null && placeSoundClip != null)
                audioSource.PlayOneShot(placeSoundClip);
            if (resultText != null) resultText.text = "Nice!";
        }
        else
        {
            if (resultText != null) resultText.text = "It fell!";
            yield return StartCoroutine(FallOff(rt));
        }

        yield return new WaitForSeconds(0.4f);
        _lastRoundSuccess = success;
    }

    private IEnumerator FallOff(RectTransform rt)
    {
        if (rt == null) yield break;

        float elapsed = 0f;
        Vector2 startPos = rt.anchoredPosition;

        while (elapsed < fallDuration && rt != null)
        {
            elapsed += Time.deltaTime;
            float newY = startPos.y - fallSpeed * elapsed;
            rt.anchoredPosition = new Vector2(startPos.x, newY);
            rt.rotation = Quaternion.Euler(0f, 0f, -elapsed * 180f);
            yield return null;
        }

        if (rt != null) rt.gameObject.SetActive(false);
    }

    private IEnumerator FailSequence()
    {
        if (audioSource != null && failClip != null)
            audioSource.PlayOneShot(failClip);

        if (resultText != null) resultText.text = "It collapsed!";
        if (tierText != null) tierText.text = "";

        StoryFlags.Instance?.SetBeerMatStackScore(LastScore);

        yield return new WaitForSeconds(2f);
        EndGame();
    }

    private void EndGame()
    {
        foreach (var m in spawnedMats) if (m != null) Destroy(m);
        spawnedMats.Clear();

        if (gamePanel != null) gamePanel.SetActive(false);
        if (PauseManager.Instance != null) PauseManager.Instance.HideCursorPublic();

        isPlaying = false;
        IsPlaying = false;
    }
}