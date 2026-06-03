using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Torch")]
    [SerializeField] private GameObject playerTorch;

    [Header("Squeaky Toy")]
    [SerializeField] private AudioSource squeakyToyAudio;
    [SerializeField] private AudioClip squeakyToyClip;
    [SerializeField] private Dog brinkley;

    [Header("UI")]
    [SerializeField] private InventoryWheel inventoryWheel;

    private List<InventoryItemData> items = new List<InventoryItemData>();
    private InventoryItemData activeItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            inventoryWheel.ToggleWheel(items);

        if (Input.GetKeyDown(KeyCode.F))
            UseActiveItem();
    }

    public void AddItem(InventoryItemData item)
    {
        items.Add(item);
        Debug.Log($"[Inventory] Added {item.itemName}");

        // Enable torch on player when picked up
        if (item.itemType == ItemType.Torch && playerTorch != null)
            playerTorch.SetActive(true);

        // Update wheel if open
        inventoryWheel.RefreshIfOpen(items);
    }

    public bool HasItem(ItemType type)
    {
        return items.Exists(i => i.itemType == type);
    }

    public void SetActiveItem(InventoryItemData item)
    {
        activeItem = item;
        Debug.Log($"[Inventory] Active item set to {item.itemName}");
    }

    private void UseActiveItem()
    {
        if (activeItem == null) return;

        switch (activeItem.itemType)
        {
            case ItemType.Torch:
                // ElectricTorchOnOff handles F key itself — nothing needed here
                break;

            case ItemType.SqueakyToy:
                UseSqueakyToy();
                break;

            case ItemType.Knife:
                // Stub — wire up to killer encounter later
                Debug.Log("[Inventory] Knife used");
                break;

            case ItemType.Collar:
                // Stub — wire up to specific scene use later
                Debug.Log("[Inventory] Collar used");
                break;
        }
    }

   private void UseSqueakyToy()
{
    if (squeakyToyAudio != null && squeakyToyClip != null)
        squeakyToyAudio.PlayOneShot(squeakyToyClip);

    if (brinkley != null)
        brinkley.ComeToPlayer();

    if (StoryFlags.Instance != null)
        StoryFlags.Instance.SetUsedSqueakyToy();

    Debug.Log("[Inventory] Squeaky toy used — calling Brinkley");
}
}