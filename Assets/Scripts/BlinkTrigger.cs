using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image vignetteImage;

    [Header("Blink Settings")]
    [SerializeField] private int blinkCount = 2;
    [SerializeField] private float fadeOutTime = 0.15f;
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float timeBetweenBlinks = 0.4f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Car") && !other.CompareTag("Player")) return;

        hasTriggered = true;
        StartCoroutine(BlinkSequence());
    }

    private IEnumerator BlinkSequence()
    {
        for (int i = 0; i < blinkCount; i++)
        {
            // Blink shut
            yield return StartCoroutine(FadeVignette(0f, 1f, fadeOutTime));
            // Blink open
            yield return StartCoroutine(FadeVignette(1f, 0f, fadeInTime));

            if (i < blinkCount - 1)
                yield return new WaitForSeconds(timeBetweenBlinks);
        }
    }

    private IEnumerator FadeVignette(float from, float to, float duration)
    {
        if (vignetteImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            vignetteImage.color = new Color(0, 0, 0, Mathf.Lerp(from, to, t));
            yield return null;
        }

        vignetteImage.color = new Color(0, 0, 0, to);
    }
}