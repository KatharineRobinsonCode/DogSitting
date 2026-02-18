using UnityEngine;

public class CanvasDebugger : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("<color=green>InteractionPromptCanvas ENABLED</color>");
        Debug.Log($"Called from: {System.Environment.StackTrace}");
    }

    void OnDisable()
    {
        Debug.Log("<color=red>InteractionPromptCanvas DISABLED!</color>");
        Debug.Log($"Called from: {System.Environment.StackTrace}");
    }
}