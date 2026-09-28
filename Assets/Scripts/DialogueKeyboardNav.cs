using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;
using System.Collections.Generic;

public class DialogueKeyboardNav : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField] private Color highlightColor = new Color(1f, 0.8f, 0f, 0.8f);
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color highlightTextColor = Color.black;

    private List<Button> optionButtons = new List<Button>();
    private int selectedIndex = 0;
    private bool optionsActive = false;
    private DialogueRunner dialogueRunner;
    private OptionsListView optionsListView;

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        optionsListView = FindFirstObjectByType<OptionsListView>();
    }

    private void Update()
    {
        if (dialogueRunner == null || !dialogueRunner.IsDialogueRunning) return;

        // Scan for active option buttons each frame when dialogue is running
        RefreshOptionButtons();

        if (!optionsActive || optionButtons.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            selectedIndex = (selectedIndex - 1 + optionButtons.Count) % optionButtons.Count;
            UpdateHighlight();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            selectedIndex = (selectedIndex + 1) % optionButtons.Count;
            UpdateHighlight();
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedIndex < optionButtons.Count && optionButtons[selectedIndex] != null)
                optionButtons[selectedIndex].onClick.Invoke();
        }
    }

    private void RefreshOptionButtons()
    {
        // Find all active interactable buttons in the options list view
        List<Button> found = new List<Button>();

        if (optionsListView != null)
        {
            Button[] buttons = optionsListView.GetComponentsInChildren<Button>(false);
            foreach (Button b in buttons)
            {
                if (b.gameObject.activeInHierarchy && b.interactable)
                    found.Add(b);
            }
        }

        // Only update if button count changed
        if (found.Count != optionButtons.Count)
        {
            optionButtons = found;
            selectedIndex = 0;
            optionsActive = found.Count > 0;

            if (optionsActive)
                UpdateHighlight();
        }
    }

    private void UpdateHighlight()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (optionButtons[i] == null) continue;

            Image bg = optionButtons[i].GetComponent<Image>();
            TextMeshProUGUI text = optionButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            bool isSelected = i == selectedIndex;

            if (bg != null) bg.color = isSelected ? highlightColor : normalColor;
            if (text != null) text.color = isSelected ? highlightTextColor : normalTextColor;

            optionButtons[i].transform.localScale = isSelected
                ? Vector3.one * 1.05f
                : Vector3.one;
        }
    }

    public void SetOptions(List<Button> buttons)
    {
        optionButtons = buttons;
        selectedIndex = 0;
        optionsActive = buttons.Count > 0;
        UpdateHighlight();
    }

    public void ClearOptions()
    {
        optionButtons.Clear();
        optionsActive = false;
    }
}