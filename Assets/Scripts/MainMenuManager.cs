using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Yarn.Unity;

public class MainMenuManager : MonoBehaviour
{
    [Header("Name Entry")]
    public TMP_InputField nameInputField;
    public VariableStorageBehaviour variableStorage;

    [Header("Audio")]
    public AudioSource menuMusic;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (menuMusic != null && !menuMusic.isPlaying)
        {
            menuMusic.Play();
        }
    }

    // Called by "New Game" button
    public void StartNewGame()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            string playerName = nameInputField.text;

            if (variableStorage != null)
            {
                // Store the player name in Yarn's variables
                variableStorage.SetValue("$PlayerName", playerName);
                Debug.Log("Saved Name: " + playerName);
            }

            SceneManager.LoadScene("CoffeeShop");
        }
        else
        {
            Debug.LogWarning("Please enter a name first!");
        }
    }

    // Called by "Continue" button
    public void ContinueGame()
    {
        // Check if there's a saved scene
        if (PlayerPrefs.HasKey("LastScene"))
        {
            string lastScene = PlayerPrefs.GetString("LastScene");
            SceneManager.LoadScene(lastScene);
        }
        else
        {
            // No save found, start new game
            Debug.Log("No save found, starting new game");
            StartNewGame();
        }
    }

    // Called by "Exit" button
    public void ExitGame()
    {
        Debug.Log("Exiting game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Volume slider in options menu
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    // Mouse sensitivity slider in options
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        Debug.Log("Sensitivity set to: " + sensitivity);
    }

    // Resolution dropdown in options
    public void SetResolution(int index)
    {
        switch (index)
        {
            case 0: Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow); break;
            case 1: Screen.SetResolution(1280, 720, FullScreenMode.FullScreenWindow); break;
            case 2: Screen.SetResolution(1280, 800, FullScreenMode.FullScreenWindow); break; // Steam Deck native
            case 3: Screen.SetResolution(2560, 1440, FullScreenMode.FullScreenWindow); break;
            case 4: Screen.SetResolution(3840, 2160, FullScreenMode.FullScreenWindow); break;
        }

        Debug.Log("Resolution changed to option: " + index);
    }

    // Save the current scene name into PlayerPrefs
    public static void SaveCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Don't save the main menu as the "last scene"
        if (currentScene != "MainMenu")
        {
            PlayerPrefs.SetString("LastScene", currentScene);
            PlayerPrefs.Save();
            Debug.Log("Saved scene: " + currentScene);
        }
    }
}

// Helper script that you can put in your game scenes (not the main menu)
// so that whenever a scene loads, it saves itself as the "last scene".
public class SceneSaveHelper : MonoBehaviour
{
    void Start()
    {
        MainMenuManager.SaveCurrentScene();
    }
}
