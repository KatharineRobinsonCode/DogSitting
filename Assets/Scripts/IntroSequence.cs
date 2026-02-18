using UnityEngine;
using System.Collections;
using TMPro;

public class IntroSequence : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup introCanvasGroup; 
    public TextMeshProUGUI introText;
    public GameObject entireIntroUI; 

    [Header("Audio")]
    public AudioSource audioSource; 
    public AudioClip typingSound;

    [Header("Typewriter Settings")]
    [TextArea(3, 10)]
    public string fullText = "The coffee shop is quiet today...";
    public float typingSpeed = 0.10f; // text speed (unchanged)

    // NEW: how often to play typing sounds (seconds between clicks)
    public float typingSoundInterval = 0.03f;

    // NEW: pitch range for variation
    public float typingPitchMin = 1.1f;
    public float typingPitchMax = 1.4f;

    [Header("Fade Settings")]
    public float fadeDuration = 2.0f;
    public float stayVisibleTime = 2.0f;

    private bool isSkipping = false;
    private bool typingFinished = false;

    // internal timer for sound spacing
    private float nextTypingSoundTime = 0f;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (introText != null) introText.text = ""; 
        if (introCanvasGroup != null) introCanvasGroup.alpha = 1f;

        StartCoroutine(RunIntroSequence());
    }

    void Update()
    {
        if (!typingFinished && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)))
        {
            isSkipping = true;
        }
    }

    IEnumerator RunIntroSequence()
    {
        yield return new WaitForSeconds(0.4f);

        // reset timer for sounds
        nextTypingSoundTime = Time.unscaledTime;

        foreach (char letter in fullText.ToCharArray())
        {
            introText.text += letter;

            // TEXT timing
            if (!isSkipping)
            {
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            // SOUND timing – independent of typingSpeed
            if (audioSource != null && typingSound != null && !isSkipping)
            {
                // only play if enough real time has passed
                if (Time.unscaledTime >= nextTypingSoundTime)
                {
                    nextTypingSoundTime = Time.unscaledTime + typingSoundInterval;

                    audioSource.Stop();
                    audioSource.time = Random.Range(0f, typingSound.length - 0.1f);
                    audioSource.pitch = Random.Range(typingPitchMin, typingPitchMax);
                    audioSource.Play();
                }
            }
        }

        typingFinished = true;
        if (audioSource != null) audioSource.Stop();

        float finalWait = isSkipping ? 0.5f : stayVisibleTime;
        yield return new WaitForSecondsRealtime(finalWait);

        float currentTime = 0f;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.unscaledDeltaTime;
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            yield return null;
        }

        if (entireIntroUI != null) 
            entireIntroUI.SetActive(false);
    }
}
