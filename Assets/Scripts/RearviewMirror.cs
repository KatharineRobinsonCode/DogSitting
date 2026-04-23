using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class RearviewMirror : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject mirrorCanvas;
    [SerializeField] private Image mirrorImage;
    [SerializeField] private Sprite[] mirrorPhotos;
    [SerializeField] private float autoCloseTime = 2f;

    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string mirrorDialogueNode = "MirrorThirdLook";

    private bool isLooking = false;
    private int photoIndex = 0;
    private bool hasTriggeredDialogue = false;

    private void Start()
    {
        if (dialogueRunner == null)
            dialogueRunner = FindFirstObjectByType<DialogueRunner>();
    }

    public string GetInteractionPrompt()
    {
        return isLooking ? "" : "Press E to check mirror";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isLooking) return;
        isLooking = true;

        if (mirrorCanvas != null)
            mirrorCanvas.SetActive(true);

        if (mirrorImage != null && mirrorPhotos.Length > 0)
        {
            int index = Mathf.Clamp(photoIndex, 0, mirrorPhotos.Length - 1);
            mirrorImage.sprite = mirrorPhotos[index];

            // Trigger dialogue on second look (index 1), once only
            if (index == 1 && !hasTriggeredDialogue && dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            {
                hasTriggeredDialogue = true;
                dialogueRunner.StartDialogue(mirrorDialogueNode);
            }

            if (photoIndex < mirrorPhotos.Length - 1)
                photoIndex++;
        }

        StartCoroutine(AutoClose());
    }

    System.Collections.IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseTime);
        if (mirrorCanvas != null)
            mirrorCanvas.SetActive(false);
        isLooking = false;
    }
}