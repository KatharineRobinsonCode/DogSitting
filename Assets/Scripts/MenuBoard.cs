using UnityEngine;

public class MenuBoard : MonoBehaviour, IInteractable
{
    [Header("UI")]
    [SerializeField] private GameObject menuPanel;

    private bool isOpen = false;

    public string GetInteractionPrompt()
    {
        return isOpen ? "Press E to close" : "Press E to read menu";
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