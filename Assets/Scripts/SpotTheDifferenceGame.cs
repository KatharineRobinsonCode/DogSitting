using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SpotTheDifferenceGame : MonoBehaviour
{
    [Header("Difference Zones")]
    [SerializeField] private Button[] differenceButtons;

    [Header("Markers")]
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private Transform leftMarkerParent;
    [SerializeField] private Transform rightMarkerParent;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI foundCounterText;

    private int differencesFound = 0;

    private void Start()
    {
        // Wire up each button to its index
        for (int i = 0; i < differenceButtons.Length; i++)
        {
            int index = i; // Cache for closure
            differenceButtons[i].onClick.AddListener(() => FoundDifference(index));
        }

        UpdateFoundCounter();
    }

    private void FoundDifference(int index)
    {
        // Disable the button so it can't be clicked again
        differenceButtons[index].interactable = false;
        differencesFound++;

        // Get the position of the clicked zone
        RectTransform zoneRect = differenceButtons[index].GetComponent<RectTransform>();

        // Place marker on right image
        if (markerPrefab != null && rightMarkerParent != null)
        {
            GameObject rightMarker = Instantiate(markerPrefab, rightMarkerParent);
            rightMarker.GetComponent<RectTransform>().position = zoneRect.position;
        }

        // Mirror marker on left image at same relative position
        if (markerPrefab != null && leftMarkerParent != null)
        {
            GameObject leftMarker = Instantiate(markerPrefab, leftMarkerParent);

            // Mirror the X position across to the left image
            Vector3 mirroredPos = zoneRect.position;
            mirroredPos.x -= Screen.width * 0.5f; // Offset to left image
            leftMarker.GetComponent<RectTransform>().position = mirroredPos;
        }

        UpdateFoundCounter();
    }

    private void UpdateFoundCounter()
    {
        if (foundCounterText != null)
            foundCounterText.text = $"Found: {differencesFound}/5";
    }
}