using UnityEngine;

public class MenuBoard : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel; // drag a UI Canvas panel here, NOT this object

    [Header("Prompts")]
    [SerializeField] private string openPrompt = "Press E to read";   // customise per object in Inspector
    [SerializeField] private string closePrompt = "Press E to close"; // customise per object in Inspector

    private bool isOpen = false;

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