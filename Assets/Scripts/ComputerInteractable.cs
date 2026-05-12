using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ComputerInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string miniGameSceneName = "MiniGame";
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip computerStartSound;
    [SerializeField] private float sceneLoadDelay = 1f;

    public string GetInteractionPrompt()
    {
        if (TaskManager.Instance == null) return "";
        if (!TaskManager.Instance.IsCurrentTask("Wait for the Pizza")) return "";
        return "Press E to play a game";
    }

  public void Interact(PlayerInteraction player)
{
    if (TaskManager.Instance == null) return;
    if (!TaskManager.Instance.IsCurrentTask("Wait for the Pizza")) return;

    HouseSceneState.SaveState(
        player.transform.position,
        player.transform.rotation
    );

    if (audioSource != null && computerStartSound != null)
        audioSource.PlayOneShot(computerStartSound);

    StartCoroutine(LoadSceneAfterDelay());
}

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(miniGameSceneName);
    }
}