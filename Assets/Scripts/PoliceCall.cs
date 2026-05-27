using UnityEngine;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;

public class PoliceCall : MonoBehaviour
{
    [Header("Phone UI")]
    [SerializeField] private GameObject phonePanel;
    [SerializeField] private UnityEngine.UI.Image policeImage;

    [Header("Audio")]
    [SerializeField] private AudioSource phoneAudio;
    [SerializeField] private AudioClip ringClip;
    [SerializeField] private AudioClip muffledCallClip;

    [Header("Dialogue")]
    [SerializeField] private string policeDialogueNode = "BreathingTought";

    private DialogueRunner dialogueRunner;
    private System.Action onComplete;

    private void Start()
    {
        if (phonePanel != null)
            phonePanel.SetActive(false);

        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public void Begin(System.Action onCallComplete)
    {
        onComplete = onCallComplete;
        StartCoroutine(PoliceCallSequence());
    }

    private IEnumerator PoliceCallSequence()
    {
        // Show phone panel
     if (phonePanel != null)
    {
        Transform parent = phonePanel.transform.parent;
        if (parent != null)
            parent.gameObject.SetActive(true);
            
        phonePanel.SetActive(true);
    }
        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowCursorPublic();

        // Play ring once
        if (phoneAudio != null && ringClip != null)
        {
            phoneAudio.PlayOneShot(ringClip);
            yield return new WaitForSeconds(ringClip.length);
        }

        // Start muffled call audio looping in background
        if (phoneAudio != null && muffledCallClip != null)
        {
            phoneAudio.clip = muffledCallClip;
            phoneAudio.loop = true;
            phoneAudio.Play();
        }

        // Start police dialogue
        if (dialogueRunner != null)
        {
            Canvas canvasComponent = dialogueRunner.GetComponentInChildren<Canvas>(true);
            if (canvasComponent != null)
            {
                canvasComponent.gameObject.SetActive(true);
                canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasGroup group = canvasComponent.gameObject.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
            }

            bool dialogueDone = false;
            dialogueRunner.onDialogueComplete.AddListener(() => dialogueDone = true);
            dialogueRunner.StartDialogue(policeDialogueNode);

            while (!dialogueDone) yield return null;

            dialogueRunner.onDialogueComplete.RemoveListener(() => dialogueDone = true);
        }

        // Stop muffled audio
        if (phoneAudio != null)
        {
            phoneAudio.loop = false;
            phoneAudio.Stop();
        }

        // Hide phone panel
        if (phonePanel != null)
            phonePanel.SetActive(false);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();

        // Notify WindowNPC call is done
        onComplete?.Invoke();
    }
}