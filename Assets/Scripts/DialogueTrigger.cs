using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Content")]
    [TextArea(3, 10)] // This makes the text box bigger and easier to type in!
    public string[] lines;           

    // We keep these variables so the Player script can access them
    [HideInInspector] public int index = 0;

    public string GetNextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            return lines[index];
        }
        return null; // Returns null to signal the conversation is over
    }

    public void ResetDialogue()
    {
        index = 0;
    }
}