using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance;
    public string currentRequiredItem;
    
    [Header("UI References")]
    public GameObject orderPanel;
    public TextMeshProUGUI orderDisplay;
    
    private Queue<string> orderQueue = new Queue<string>();
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    void Start()
    {
        if (orderPanel != null) orderPanel.SetActive(false);
    }
    
    public void ShowOrder(string orderText)
    {
        if (orderPanel != null)
        {
            orderPanel.SetActive(true);
            if (orderDisplay != null)
            {
                orderDisplay.text = orderText;
                currentRequiredItem = orderText.Replace("Order: ", "").Trim();
            }
        }
        
        // Update task box
        TaskManager.Instance?.ShowTask("Make " + currentRequiredItem);
    }
    
    public void HideOrder()
    {
        if (orderPanel != null)
            orderPanel.SetActive(false);
        
        // Clear task box
        TaskManager.Instance?.HideTask();
    }

    public int customersServed = 0;
    public Sprite creepyPhoto;

    public void CustomerLeft()
    {
        customersServed++;

    }
}