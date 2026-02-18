using UnityEngine;
using UnityEngine.SceneManagement;

// We add IInteractable so the Player script recognizes the door
public class DoorEnding : MonoBehaviour, IInteractable
{
    public GameObject endingCanvas; 
    public float interactDistance = 3f;
    public Transform player;

    [Header("Ending Text")]
    public TMPro.TextMeshProUGUI endingText;
    [TextArea(3, 10)]
    public string secretEndingMessage = "ENDING 1/5 You left your shift early. You missed an important text on the drive home because your boss fired you... Don't you wish you stayed so you could meet the dog?";

    private bool isEndingTriggered = false;

    // --- These functions let the PlayerInteraction script "talk" to the door ---

    public string GetInteractionPrompt()
    {
        return "Press E to Leave Early";
    }

    public void Interact(PlayerInteraction player)
    {
        if (!isEndingTriggered)
        {
            TriggerEnding();
        }
    }

    // --- END OF INTERFACE FUNCTIONS ---

    void TriggerEnding()
    {
        isEndingTriggered = true;
        Debug.Log("Ending Triggered! Attempting to show UI...");

        // If not set in Inspector, try to find it by name
        if (endingCanvas == null)
            endingCanvas = GameObject.Find("EndingPanelCanvas");

        if (endingCanvas != null)
        {
            endingCanvas.SetActive(true);

            // Make sure inner panel is active
            Transform panel = endingCanvas.transform.Find("EndingPanel");
            if (panel != null) panel.gameObject.SetActive(true);

            // Find text if missing
            if (endingText == null)
                endingText = endingCanvas.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (endingText != null)
                endingText.text = secretEndingMessage;
        }
        else
        {
            Debug.LogError("COULD NOT FIND EndingPanelCanvas! Check the name spelling in Hierarchy.");
        }

        Time.timeScale = 0f; 
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToDesktop()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
