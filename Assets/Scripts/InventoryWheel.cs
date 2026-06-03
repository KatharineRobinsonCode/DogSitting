using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryWheel : MonoBehaviour
{
    [Header("Wheel UI")]
    [SerializeField] private GameObject wheelPanel;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private float radius = 150f;

    [Header("Active Item HUD")]
    [SerializeField] private GameObject activeItemHUD;
    [SerializeField] private Image activeItemIcon;
    [SerializeField] private TextMeshProUGUI activeItemName;

    private bool isOpen = false;
    private List<GameObject> spawnedSlots = new List<GameObject>();

    private void Start()
    {
        if (wheelPanel != null) wheelPanel.SetActive(false);
        if (activeItemHUD != null) activeItemHUD.SetActive(false);
    }

    public void ToggleWheel(List<InventoryItemData> items)
    {
        isOpen = !isOpen;

        if (isOpen)
            OpenWheel(items);
        else
            CloseWheel();
    }

    public void RefreshIfOpen(List<InventoryItemData> items)
    {
        if (isOpen) OpenWheel(items);
    }

    private void OpenWheel(List<InventoryItemData> items)
    {
        wheelPanel.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Clear old slots
        foreach (var slot in spawnedSlots)
            Destroy(slot);
        spawnedSlots.Clear();

        if (items.Count == 0) return;

        // Place items in a circle
        float angleStep = 360f / items.Count;

        for (int i = 0; i < items.Count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;

            GameObject slot = Instantiate(itemSlotPrefab, slotsParent);
            slot.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

            // Set icon
            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && items[i].icon != null)
                icon.sprite = items[i].icon;

            // Set name
            TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null)
                nameText.text = items[i].itemName;

            // Set click to select
            var itemData = items[i];
            Button btn = slot.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => SelectItem(itemData));

            spawnedSlots.Add(slot);
        }
    }

    private void CloseWheel()
    {
        wheelPanel.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isOpen = false;
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