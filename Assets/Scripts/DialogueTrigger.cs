using UnityEngine;

/// <summary>
/// Simple dialogue system that stores and manages sequential dialogue lines.
/// Can be used for NPCs or interactable objects that need basic conversations.
/// </summary>
public class DialogueTrigger : MonoBehaviour
{
    #region Serialized Fields
    
    [Header("Dialogue Content")]
    [Tooltip("Lines of dialogue in sequential order")]
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogueLines = new string[0];
    
    [Header("Settings")]
    [Tooltip("Automatically reset dialogue when reaching the end")]
    [SerializeField] private bool autoResetOnComplete = false;
    
    [Tooltip("Allow dialogue to loop after completion")]
    [SerializeField] private bool loopDialogue = false;
    
    #endregion
    
    #region Private Fields
    
    private int currentIndex = 0;
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Returns the current dialogue line without advancing.
    /// </summary>
    public string CurrentLine => GetCurrentLine();
    
    /// <summary>
    /// Returns the current line index.
    /// </summary>
    public int CurrentIndex => currentIndex;
    
    /// <summary>
    /// Returns the total number of dialogue lines.
    /// </summary>
    public int TotalLines => dialogueLines.Length;
    
    /// <summary>
    /// Checks if there are more lines to read.
    /// </summary>
    public bool HasMoreLines => currentIndex < dialogueLines.Length - 1;
    
    /// <summary>
    /// Checks if the dialogue is at the first line.
    /// </summary>
    public bool IsAtStart => currentIndex == 0;
    
    /// <summary>
    /// Checks if the dialogue is at the last line.
    /// </summary>
    public bool IsAtEnd => currentIndex >= dialogueLines.Length - 1;
    
    /// <summary>
    /// Checks if dialogue has been completed.
    /// </summary>
    public bool IsComplete => currentIndex >= dialogueLines.Length;
    
    #endregion
    
    #region Initialization
    
    private void Awake()
    {
        ValidateDialogueLines();
    }
    
    private void ValidateDialogueLines()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning($"[DialogueTrigger] {gameObject.name} has no dialogue lines assigned", this);
        }
    }
    
    #endregion
    
    #region Public API - Navigation
    
    /// <summary>
    /// Gets the current line and advances to the next one.
    /// Returns null if no more lines available.
    /// </summary>
    public string GetNextLine()
    {
        if (!HasDialogueLines())
        {
            Debug.LogWarning("[DialogueTrigger] No dialogue lines available");
            return null;
        }
        
        // Get current line
        string currentLine = GetCurrentLine();
        
        // Advance index
        AdvanceToNextLine();
        
        return currentLine;
    }
    
    /// <summary>
    /// Gets the current line without advancing the index.
    /// </summary>
    public string PeekCurrentLine()
    {
        return GetCurrentLine();
    }
    
    /// <summary>
    /// Gets the next line without advancing the index.
    /// Returns null if at the end.
    /// </summary>
    public string PeekNextLine()
    {
        if (HasMoreLines)
        {
            return dialogueLines[currentIndex + 1];
        }
        
        return null;
    }
    
    /// <summary>
    /// Advances to the next line without returning text.
    /// </summary>
    public void AdvanceDialogue()
    {
        AdvanceToNextLine();
    }
    
    /// <summary>
    /// Goes back to the previous line if possible.
    /// </summary>
    public void GoToPreviousLine()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
        }
    }
    
    /// <summary>
    /// Skips to a specific line index.
    /// </summary>
    public void SkipToLine(int lineIndex)
    {
        if (lineIndex >= 0 && lineIndex < dialogueLines.Length)
        {
            currentIndex = lineIndex;
        }
        else
        {
            Debug.LogWarning($"[DialogueTrigger] Invalid line index: {lineIndex}");
        }
    }
    
    #endregion
    
    #region Public API - Control
    
    /// <summary>
    /// Resets the dialogue to the beginning.
    /// </summary>
    public void ResetDialogue()
    {
        currentIndex = 0;
    }
    
    /// <summary>
    /// Starts the dialogue from the beginning.
    /// Alias for ResetDialogue for clarity.
    /// </summary>
    public void StartDialogue()
    {
        ResetDialogue();
    }
    
    /// <summary>
    /// Marks the dialogue as complete by moving past the last line.
    /// </summary>
    public void CompleteDialogue()
    {
        currentIndex = dialogueLines.Length;
    }
    
    #endregion
    
    #region Private Methods
    
    private bool HasDialogueLines()
    {
        return dialogueLines != null && dialogueLines.Length > 0;
    }
    
    private string GetCurrentLine()
    {
        if (!HasDialogueLines())
        {
            return string.Empty;
        }
        
        if (currentIndex >= 0 && currentIndex < dialogueLines.Length)
        {
            return dialogueLines[currentIndex];
        }
        
        return string.Empty;
    }
    
    private void AdvanceToNextLine()
    {
        if (currentIndex < dialogueLines.Length - 1)
        {
            currentIndex++;
        }
        else
        {
            HandleDialogueEnd();
        }
    }
    
    private void HandleDialogueEnd()
    {
        if (loopDialogue)
        {
            ResetDialogue();
        }
        else if (autoResetOnComplete)
        {
            ResetDialogue();
        }
        else
        {
            // Mark as complete (move past last line)
            currentIndex = dialogueLines.Length;
        }
    }
    
    #endregion
    
    #region Public Utility Methods
    
    /// <summary>
    /// Returns all dialogue lines as an array.
    /// </summary>
    public string[] GetAllLines()
    {
        return dialogueLines;
    }
    
    /// <summary>
    /// Gets a specific line by index.
    /// Returns null if index is out of range.
    /// </summary>
    public string GetLineAt(int index)
    {
        if (index >= 0 && index < dialogueLines.Length)
        {
            return dialogueLines[index];
        }
        
        return null;
    }
    
    /// <summary>
    /// Sets new dialogue lines and resets the index.
    /// </summary>
    public void SetDialogueLines(string[] newLines)
    {
        dialogueLines = newLines;
        ResetDialogue();
        ValidateDialogueLines();
    }
    
    /// <summary>
    /// Returns the progress through the dialogue as a percentage (0-1).
    /// </summary>
    public float GetProgressPercentage()
    {
        if (dialogueLines.Length == 0)
        {
            return 0f;
        }
        
        return (float)currentIndex / (dialogueLines.Length - 1);
    }
    
    #endregion
    
    #region Debug Helpers
    
    /// <summary>
    /// Logs the current dialogue state to the console.
    /// </summary>
    [ContextMenu("Log Dialogue State")]
    public void LogDialogueState()
    {
        Debug.Log("=== DIALOGUE STATE ===");
        Debug.Log($"Current Index: {currentIndex}");
        Debug.Log($"Total Lines: {dialogueLines.Length}");
        Debug.Log($"Has More Lines: {HasMoreLines}");
        Debug.Log($"Is Complete: {IsComplete}");
        Debug.Log($"Progress: {GetProgressPercentage():P0}");
        
        if (!string.IsNullOrEmpty(CurrentLine))
        {
            Debug.Log($"Current Line: \"{CurrentLine}\"");
        }
        
        Debug.Log("====================");
    }
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Remove empty lines at the end
        if (dialogueLines != null && dialogueLines.Length > 0)
        {
            int lastValidIndex = dialogueLines.Length - 1;
            while (lastValidIndex >= 0 && string.IsNullOrEmpty(dialogueLines[lastValidIndex]))
            {
                lastValidIndex--;
            }
            
            if (lastValidIndex < dialogueLines.Length - 1)
            {
                System.Array.Resize(ref dialogueLines, lastValidIndex + 1);
            }
        }
    }
    #endif
    
    #endregion
}