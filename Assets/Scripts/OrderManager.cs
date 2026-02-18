using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages customer orders and UI display.
/// Tracks order completion and triggers horror events at specific milestones.
/// </summary>
public class OrderManager : MonoBehaviour
{
    #region Singleton
    
    public static OrderManager Instance { get; private set; }
    
    #endregion
    
    #region Serialized Fields
    
    [Header("Order UI")]
    [Tooltip("Background panel containing order information")]
    [SerializeField] private GameObject orderPanel;
    
    [Tooltip("Text display showing current order details")]
    [SerializeField] private TextMeshProUGUI orderDisplay;
    
    [Header("Horror Event")]
    [Tooltip("Creepy image to display after 3rd customer")]
    [SerializeField] private Sprite creepyPhoto;
    
    [Tooltip("Which customer number triggers the horror event")]
    [SerializeField] private int horrorEventTriggerCustomer = 3;
    
    [Header("Debug")]
    [Tooltip("Total customers served (read-only)")]
    [SerializeField] private int customersServed = 0;
    
    #endregion
    
    #region Private Fields
    
    private Queue<string> orderQueue = new Queue<string>();
    private string currentRequiredItem;
    
    // Constants
    private const string ORDER_PREFIX = "Order: ";
    
    #endregion
    
    #region Properties
    
    /// <summary>
    /// Public read-only access to current order requirement
    /// </summary>
    public string CurrentRequiredItem => currentRequiredItem;
    
    /// <summary>
    /// Public read-only access to customers served count
    /// </summary>
    public int CustomersServed => customersServed;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        InitializeUI();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void InitializeUI()
    {
        HideOrder();
    }
    
    #endregion
    
    #region Public API - Order Management
    
    /// <summary>
    /// Displays an order on screen and parses the required item.
    /// Called by NPCs when they finish their conversation.
    /// </summary>
    /// <param name="orderText">Full order text (e.g., "Order: 1x Coffee")</param>
    public void ShowOrder(string orderText)
    {
        if (string.IsNullOrEmpty(orderText))
        {
            Debug.LogWarning("[OrderManager] Attempted to show empty order");
            return;
        }
        
        DisplayOrderUI(orderText);
        ParseRequiredItem(orderText);
    }
    
    /// <summary>
    /// Hides the order UI from screen.
    /// Called by the Register when order is completed.
    /// </summary>
    public void HideOrder()
    {
        if (orderPanel != null)
        {
            orderPanel.SetActive(false);
        }
        
        currentRequiredItem = string.Empty;
    }
    
    /// <summary>
    /// Notifies the manager that a customer has been served and left.
    /// Triggers horror events at specific customer counts.
    /// </summary>
    public void CustomerLeft()
    {
        customersServed++;
        
        Debug.Log($"[OrderManager] Customer #{customersServed} served");
        
        CheckForHorrorEvent();
    }
    
    #endregion
    
    #region Private Methods - UI
    
    private void DisplayOrderUI(string orderText)
    {
        if (orderPanel != null)
        {
            orderPanel.SetActive(true);
        }
        
        if (orderDisplay != null)
        {
            orderDisplay.text = orderText;
        }
    }
    
    private void ParseRequiredItem(string orderText)
    {
        // Extract the actual item from "Order: 1x Coffee" → "1x Coffee"
        currentRequiredItem = orderText.Replace(ORDER_PREFIX, string.Empty).Trim();
    }
    
    #endregion
    
    #region Private Methods - Horror Events
    
    private void CheckForHorrorEvent()
    {
        if (customersServed == horrorEventTriggerCustomer)
        {
            TriggerHorrorEvent();
        }
    }
    
    private void TriggerHorrorEvent()
    {
        if (!ValidateHorrorEventRequirements())
        {
            return;
        }
        
        Debug.Log($"[OrderManager] Triggering horror event after {horrorEventTriggerCustomer} customers");
        
        PhoneManager.Instance.ReceiveAirdrop(
            content: "AirDrop: 'I LIKE YOUR SHIRT.'",
            horrorPhoto: creepyPhoto
        );
    }
    
    private bool ValidateHorrorEventRequirements()
    {
        if (PhoneManager.Instance == null)
        {
            Debug.LogError("[OrderManager] Cannot trigger horror event: PhoneManager not found!");
            return false;
        }
        
        if (creepyPhoto == null)
        {
            Debug.LogWarning("[OrderManager] Cannot trigger horror event: Creepy photo not assigned!");
            return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Future - Queue System (Not Currently Used)
    
    // Note: The orderQueue field is declared but not currently used.
    // This could be extended in the future to handle multiple pending orders.
    
    /// <summary>
    /// Adds an order to the queue for future processing.
    /// Currently not implemented in gameplay.
    /// </summary>
    private void EnqueueOrder(string order)
    {
        orderQueue.Enqueue(order);
    }
    
    /// <summary>
    /// Retrieves the next order from the queue.
    /// Currently not implemented in gameplay.
    /// </summary>
    private string DequeueOrder()
    {
        return orderQueue.Count > 0 ? orderQueue.Dequeue() : null;
    }
    
    #endregion
}