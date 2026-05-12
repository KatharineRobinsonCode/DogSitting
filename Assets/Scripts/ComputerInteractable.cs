using UnityEngine;
using UnityEngine.SceneManagement;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string miniGameSceneName = "MiniGame";
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip computerStartSound;

    public string GetInteractionPrompt()
    {
        if (!TaskManager.Instance.IsCurrentTask("Wait for the Pizza")) return "";
        return "Press E to play a game";
    }

    public void Interact(PlayerInteraction player)
    {
        if (!TaskManager.Instance.IsCurrentTask("Wait for the Pizza")) return;

        if (audioSource != null && computerStartSound != null)
            audioSource.PlayOneShot(computerStartSound);

        SceneManager.LoadScene(miniGameSceneName);
    }
}