using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class RearviewMirror : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject mirrorCanvas;
    [SerializeField] private Image mirrorImage;
    [SerializeField] private Sprite[] mirrorPhotos;
    [SerializeField] private float autoCloseTime = 3f;

    [Header("Dialogue")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private string thirdLookNode = "MirrorThirdLook";

    private bool isLooking = false;
    private int photoIndex = 0;

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

            // Trigger dialogue on third look (index 2)
            if (index == 2 && dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
            {
                dialogueRunner.StartDialogue(thirdLookNode);
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