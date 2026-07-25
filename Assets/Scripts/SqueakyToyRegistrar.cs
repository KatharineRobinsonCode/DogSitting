using UnityEngine;

public class SqueakyToyRegistrar : MonoBehaviour
{
    private void Start()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetSqueakyToyAudio(GetComponent<AudioSource>());
    }
}