using UnityEngine;
using TMPro;
using Yarn.Unity;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages player name input and initialization.
/// Stores the name in Yarn Spinner's variable system for use in dialogue.
/// </summary>
public class NameManager : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("UI References")]
    [Tooltip("Input field where player enters their name")]
    [SerializeField] private TMP_InputField nameInputField;
    
    [Header("Yarn Integration")]
    [Tooltip("Yarn Spinner's variable storage system")]
    [SerializeField] private VariableStorageBehaviour variableStorage;
    
    [Header("Scene Settings")]
    [Tooltip("Scene to load after name entry (leave empty to stay in current scene)")]
    [SerializeField] private string targetSceneName = string.Empty;
    
    #endregion
    
    #region Private Fields
    
    private string playerName;
    
    // Constants
    private const string YARN_PLAYER_NAME_VARIABLE = "$PlayerName";
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Public read-only access to the player's name
    /// </summary>
    public string PlayerName => playerName;
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Validates name input, saves to Yarn variables, and proceeds to game.
    /// Called by UI button click.
    /// </summary>
    public void SaveNameAndStart()
    {
        if (!ValidateNameInput())
        {
            return;
        }
        
        SavePlayerName();
        StoreNameInYarn();
        ProceedToGame();
    }
    
    #endregion
    
    #region Validation
    
    private bool ValidateNameInput()
    {
        if (nameInputField == null)
        {
            Debug.LogError("[NameManager] Name input field not assigned!");
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(nameInputField.text))
        {
            Debug.LogWarning("[NameManager] Player name is empty");
            return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Name Management
    
    private void SavePlayerName()
    {
        playerName = nameInputField.text.Trim();
        Debug.Log($"[NameManager] Player name set to: {playerName}");
    }
    
    private void StoreNameInYarn()
    {
        if (variableStorage == null)
        {
            Debug.LogWarning("[NameManager] Variable storage not assigned. Name won't be available in Yarn dialogues.");
            return;
        }
        
        try
        {
            variableStorage.SetValue(YARN_PLAYER_NAME_VARIABLE, playerName);
            Debug.Log($"[NameManager] Name stored in Yarn variable: {YARN_PLAYER_NAME_VARIABLE} = {playerName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NameManager] Failed to store name in Yarn: {e.Message}");
        }
    }
    
    #endregion
    
    #region Game Flow
    
    private void ProceedToGame()
    {
        if (ShouldLoadNewScene())
        {
            LoadTargetScene();
        }
        else
        {
            CloseNamePanel();
        }
        
        RestoreCursorForGameplay();
    }
    
    private bool ShouldLoadNewScene()
    {
        return !string.IsNullOrEmpty(targetSceneName);
    }
    
    private void LoadTargetScene()
    {
        Debug.Log($"[NameManager] Loading scene: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }
    
    private void CloseNamePanel()
    {
        gameObject.SetActive(false);
    }
    
    private void RestoreCursorForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    #endregion
    
    #region Optional Public Utilities
    
    /// <summary>
    /// Programmatically set player name (bypassing UI input)
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogWarning("[NameManager] Attempted to set empty name");
            return;
        }
        
        playerName = name.Trim();
        StoreNameInYarn();
    }
    
    /// <summary>
    /// Clear the current player name
    /// </summary>
    public void ClearPlayerName()
    {
        playerName = string.Empty;
        
        if (nameInputField != null)
        {
            nameInputField.text = string.Empty;
        }
        
        if (variableStorage != null)
        {
            variableStorage.SetValue(YARN_PLAYER_NAME_VARIABLE, string.Empty);
        }
    }
    
    #endregion
}