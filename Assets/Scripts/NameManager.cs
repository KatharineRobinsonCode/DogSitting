using UnityEngine;
using TMPro;
using Yarn.Unity;
using UnityEngine.SceneManagement;

public class NameManager : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public string playerName;
    
    // Reference to Yarn Spinner's Variable Storage
    public VariableStorageBehaviour variableStorage;

    public void SaveNameAndStart()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            playerName = nameInputField.text;

            // PUSH THE NAME TO YARN
            // "$PlayerName" is the variable name inside your Yarn scripts
            if (variableStorage != null)
            {
                variableStorage.SetValue("$PlayerName", playerName);
                Debug.Log("Player name saved to Yarn: " + playerName);
            }

            // Close the panel or Load your game scene
            // SceneManager.LoadScene("CafeScene"); 
            // Or just deactivate the menu if you're already in the scene:
            this.gameObject.SetActive(false);
            
            // Re-lock the cursor so the player can play
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}