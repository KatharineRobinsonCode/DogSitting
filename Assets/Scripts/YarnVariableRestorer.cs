using UnityEngine;
using Yarn.Unity;
using System.Collections;

public class YarnVariableRestorer : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;  // wait one frame for DialogueRunner to initialise

        InMemoryVariableStorage storage = FindFirstObjectByType<InMemoryVariableStorage>();
        if (storage == null)
        {
            Debug.LogWarning("[YarnVariableRestorer] No InMemoryVariableStorage found");
            yield break;
        }

        string playerName = PlayerPrefs.GetString("PlayerName", "You");
        storage.SetValue("$PlayerName", playerName);

        Debug.Log($"[YarnVariableRestorer] Restored $PlayerName: {playerName}");
    }
}