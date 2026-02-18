using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Manages the intro sequence with typewriter text effect and audio.
/// Displays opening text that can be skipped by the player.
/// </summary>
public class IntroSequence : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("UI References")]
    [Tooltip("Canvas group for fading the entire intro")]
    [SerializeField] private CanvasGroup introCanvasGroup;
    
    [Tooltip("Text component for typewriter effect")]
    [SerializeField] private TextMeshProUGUI introText;
    
    [Tooltip("Parent GameObject containing all intro UI")]
    [SerializeField] private GameObject entireIntroUI;
    
    [Header("Content")]
    [TextArea(3, 10)]
    [Tooltip("Text to display with typewriter effect")]
    [SerializeField] private string fullText = "The coffee shop is quiet today...";
    
    [Header("Typewriter Settings")]
    [Tooltip("Delay between each character appearing (seconds)")]
    [SerializeField] private float typingSpeed = 0.10f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Tooltip("Typing sound effect")]
    [SerializeField] private AudioClip typingSound;
    
    [Tooltip("Delay between typing sound plays (seconds)")]
    [SerializeField] private float typingSoundInterval = 0.03f;
    
    [Tooltip("Minimum pitch variation for typing sounds")]
    [SerializeField] private float typingPitchMin = 1.1f;
    
    [Tooltip("Maximum pitch variation for typing sounds")]
    [SerializeField] private float typingPitchMax = 1.4f;
    
    [Header("Fade Settings")]
    [Tooltip("Duration of fade out animation (seconds)")]
    [SerializeField] private float fadeDuration = 2.0f;
    
    [Tooltip("How long text stays on screen before fading (seconds)")]
    [SerializeField] private float stayVisibleTime = 2.0f;
    
    [Tooltip("Initial delay before text starts appearing (seconds)")]
    [SerializeField] private float initialDelay = 0.4f;
    
    [Tooltip("Minimum time before fade when skipped (seconds)")]
    [SerializeField] private float skipMinimumWait = 0.5f;
    
    #endregion
    
    #region Private Fields
    
    private bool isSkipping = false;
    private bool typingFinished = false;
    private float nextTypingSoundTime = 0f;
    
    // Constants
    private const float AUDIO_SAFETY_OFFSET = 0.1f;
    private const float FADE_START_ALPHA = 1f;
    private const float FADE_END_ALPHA = 0f;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeCursor();
        InitializeUI();
        StartCoroutine(RunIntroSequence());
    }
    
    private void Update()
    {
        CheckForSkipInput();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    private void InitializeUI()
    {
        if (introText != null)
        {
            introText.text = string.Empty;
        }
        
        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = FADE_START_ALPHA;
        }
    }
    
    #endregion
    
    #region Input Handling
    
    private void CheckForSkipInput()
    {
        if (typingFinished)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            SkipTypewriter();
        }
    }
    
    private void SkipTypewriter()
    {
        isSkipping = true;
        StopTypingSound();
    }
    
    #endregion
    
    #region Intro Sequence
    
    private IEnumerator RunIntroSequence()
    {
        yield return new WaitForSeconds(initialDelay);
        
        yield return StartCoroutine(TypewriterEffect());
        
        yield return StartCoroutine(WaitBeforeFade());
        
        yield return StartCoroutine(FadeOut());
        
        HideIntroUI();
    }
    
    #endregion
    
    #region Typewriter Effect
    
    private IEnumerator TypewriterEffect()
    {
        PrepareTypewriter();
        
        foreach (char letter in fullText)
        {
            DisplayCharacter(letter);
            
            if (!isSkipping)
            {
                yield return new WaitForSecondsRealtime(typingSpeed);
                PlayTypingSoundIfReady();
            }
        }
        
        FinishTypewriter();
    }
    
    private void PrepareTypewriter()
    {
        nextTypingSoundTime = Time.unscaledTime;
    }
    
    private void DisplayCharacter(char letter)
    {
        if (introText != null)
        {
            introText.text += letter;
        }
    }
    
    private void PlayTypingSoundIfReady()
    {
        if (!CanPlayTypingSound())
        {
            return;
        }
        
        if (Time.unscaledTime >= nextTypingSoundTime)
        {
            PlayTypingSound();
            nextTypingSoundTime = Time.unscaledTime + typingSoundInterval;
        }
    }
    
    private bool CanPlayTypingSound()
    {
        return audioSource != null && 
               typingSound != null && 
               !isSkipping;
    }
    
    private void PlayTypingSound()
    {
        audioSource.Stop();
        audioSource.time = GetRandomSoundStartTime();
        audioSource.pitch = GetRandomPitch();
        audioSource.Play();
    }
    
    private float GetRandomSoundStartTime()
    {
        float maxTime = typingSound.length - AUDIO_SAFETY_OFFSET;
        return Random.Range(0f, Mathf.Max(0f, maxTime));
    }
    
    private float GetRandomPitch()
    {
        return Random.Range(typingPitchMin, typingPitchMax);
    }
    
    private void FinishTypewriter()
    {
        typingFinished = true;
        StopTypingSound();
    }
    
    private void StopTypingSound()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
    
    #endregion
    
    #region Wait and Fade
    
    private IEnumerator WaitBeforeFade()
    {
        float waitTime = isSkipping ? skipMinimumWait : stayVisibleTime;
        yield return new WaitForSecondsRealtime(waitTime);
    }
    
    private IEnumerator FadeOut()
    {
        if (introCanvasGroup == null)
        {
            yield break;
        }
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / fadeDuration;
            
            introCanvasGroup.alpha = Mathf.Lerp(FADE_START_ALPHA, FADE_END_ALPHA, progress);
            
            yield return null;
        }
        
        introCanvasGroup.alpha = FADE_END_ALPHA;
    }
    
    private void HideIntroUI()
    {
        if (entireIntroUI != null)
        {
            entireIntroUI.SetActive(false);
        }
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Immediately completes and hides the intro sequence.
    /// </summary>
    public void ForceSkip()
    {
        StopAllCoroutines();
        
        isSkipping = true;
        typingFinished = true;
        
        if (introText != null)
        {
            introText.text = fullText;
        }
        
        if (introCanvasGroup != null)
        {
            introCanvasGroup.alpha = FADE_END_ALPHA;
        }
        
        HideIntroUI();
    }
    
    /// <summary>
    /// Check if the intro sequence has finished.
    /// </summary>
    public bool IsComplete()
    {
        return typingFinished && (entireIntroUI == null || !entireIntroUI.activeSelf);
    }
    
    #endregion
}