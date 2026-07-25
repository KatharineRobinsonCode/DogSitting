using UnityEngine;
using Yarn.Unity;

public class YarnVariableRestorer : MonoBehaviour
{
    private void Start()
    {
        InMemoryVariableStorage storage = FindFirstObjectByType<InMemoryVariableStorage>();
        if (storage == null) return;

        string playerName = PlayerPrefs.GetString("PlayerName", "You");
        storage.SetValue("$PlayerName", playerName);

        Debug.Log($"[YarnVariableRestorer] Restored $PlayerName: {playerName}");
    }
}