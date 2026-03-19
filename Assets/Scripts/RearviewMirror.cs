using UnityEngine;
using UnityEngine.UI;

public class RearviewMirror : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject mirrorCanvas;
    [SerializeField] private Image mirrorImage;
    [SerializeField] private Sprite[] mirrorPhotos; // drag creepy car photos in
    [SerializeField] private float autoCloseTime = 3f;
    
    private bool isLooking = false;
    private int photoIndex = 0;

    public string GetInteractionPrompt()
    {
        if (!isLooking)
            return "Press E to check mirror";
        return "";
    }

    public void Interact(PlayerInteraction player)
    {
        if (isLooking) return;
        isLooking = true;

        if (mirrorCanvas != null)
            mirrorCanvas.SetActive(true);

        // Show next photo in sequence
        if (mirrorImage != null && mirrorPhotos.Length > 0)
        {
            photoIndex = Mathf.Min(photoIndex, mirrorPhotos.Length - 1);
            mirrorImage.sprite = mirrorPhotos[photoIndex];
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