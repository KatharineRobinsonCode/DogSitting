using UnityEngine;

public class MenuBoard : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel; // drag a UI Canvas panel here, NOT this object

    [Header("Prompts")]
    [SerializeField] private string openPrompt = "Press E to read";   // customise per object in Inspector
    [SerializeField] private string closePrompt = "Press E to close"; // customise per object in Inspector
    
[Header("References")]
[SerializeField] private GameObject interactionPrompt; // drag your InteractionPrompt GameObject here

    private bool isOpen = false;
private void Update()
{
    // Force the close prompt to show whenever the panel is open
    if (isOpen && interactionPrompt != null)
    {
        interactionPrompt.SetActive(true);
        // Find the TMP text and set it
        TMPro.TextMeshProUGUI text = interactionPrompt.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null) text.text = closePrompt;
    }
}
    public string GetInteractionPrompt()
    {
        return isOpen ? closePrompt : openPrompt;
    }

    public void Interact(PlayerInteraction player)
    {
        if (isOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OpenMenu()
    {
        isOpen = true;
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (PauseManager.Instance != null)
            PauseManager.Instance.ShowCursorPublic();
    }

    private void CloseMenu()
    {
        isOpen = false;
        if (menuPanel != null)
            menuPanel.SetActive(false);

        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }
}