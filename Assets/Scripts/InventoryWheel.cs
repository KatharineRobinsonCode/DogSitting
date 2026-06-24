using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryWheel : MonoBehaviour
{
    [Header("Slot Prefab")]
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform slotsParent;

    [Header("Active Item HUD")]
    [SerializeField] private GameObject activeItemHUD;
    [SerializeField] private Image activeItemIcon;
    [SerializeField] private TextMeshProUGUI activeItemName;
    
    [Header("Slot Positions")]
    [SerializeField] private RectTransform[] slotPositions;

    private bool isOpen = false;
    private List<GameObject> spawnedSlots = new List<GameObject>();

    private void Start()
    {
        if (activeItemHUD != null) activeItemHUD.SetActive(false);
    }

    public void ToggleWheel(List<InventoryItemData> items)
    {
        if (!isOpen && items.Count == 0) return;

        isOpen = !isOpen;

        if (isOpen)
            OpenWheel(items);
        else
            CloseWheel();
    }

    public void RefreshIfOpen(List<InventoryItemData> items)
    {
        if (!isOpen) return;
        OpenWheel(items);
    }

    private void OpenWheel(List<InventoryItemData> items)
    {
        foreach (var slot in spawnedSlots)
            Destroy(slot);
        spawnedSlots.Clear();

        // PhoneManager handles canvas and cursor
        PhoneManager.Instance.OpenInventory();

        if (items.Count == 0) return;

        for (int i = 0; i < items.Count; i++)
        {
            if (i >= slotPositions.Length) break;

            GameObject slot = Instantiate(itemSlotPrefab, slotsParent);
            slot.GetComponent<RectTransform>().anchoredPosition =
                slotPositions[i].anchoredPosition;

            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && items[i].icon != null)
                icon.sprite = items[i].icon;

            TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = items[i].itemName;

            var itemData = items[i];
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SelectItem(itemData));

            spawnedSlots.Add(slot);
        }
    }

    private void CloseWheel()
    {
        isOpen = false;
        // PhoneManager handles canvas and cursor
        PhoneManager.Instance.CloseInventory();
    }

    private void SelectItem(InventoryItemData item)
    {
        InventoryManager.Instance.SetActiveItem(item);

        if (activeItemHUD != null) activeItemHUD.SetActive(true);
        if (activeItemIcon != null && item.icon != null) activeItemIcon.sprite = item.icon;
        if (activeItemName != null) activeItemName.text = item.itemName;

        CloseWheel();
    }
}