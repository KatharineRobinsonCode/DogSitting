using UnityEngine;
using TMPro;
using Yarn.Unity;

public class PlayerInteraction : MonoBehaviour
{
    #region Serialized Fields

    [Header("Interaction Settings")]
    [SerializeField] private float interactDistance = 5f;
    [SerializeField] private LayerMask excludeLayers;

    [Header("UI References")]
    [SerializeField] private GameObject interactionPromptCanvas;
    [SerializeField] private GameObject promptObject;
    [SerializeField] private UnityEngine.UI.Image crosshairImage;

    [Header("Crosshair Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color interactColor = Color.yellow;
    [SerializeField] private float interactScale = 1.2f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float pulseIntensity = 0.1f;

    [Header("Item Holding")]
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Vector3 holdPosition = new Vector3(0.4f, -0.4f, 0.7f);
    [SerializeField] private Vector3 holdRotation = new Vector3(30, 0, 0);

    [Header("Debug")]
    [SerializeField] private GameObject currentHeldItem;

    #endregion

    #region Private Fields

    private bool justPickedUpBroom = false;
    private TextMeshProUGUI promptText;
    private DialogueRunner dialogueRunner;
    private int originalItemLayer;

    private const string HELD_ITEMS_LAYER = "HeldItems";
    private const KeyCode INTERACT_KEY = KeyCode.E;

    #endregion

    #region Properties

    public GameObject CurrentHeldItem => currentHeldItem;
    public bool IsHoldingBroom { get; private set; }

    public void SetHoldingBroom(bool value)
    {
        IsHoldingBroom = value;
        if (value) justPickedUpBroom = true;
    }

    #endregion

    #region Unity Lifecycle

    private void Awake() { EnsureCanvasActive(); }

    private void Start()
    {
        InitializeComponents();
        EnsureCanvasActive();
    }

    private void Update()
    {
        if (!CanInteract())
        {
            UpdateUI(false, string.Empty);
            return;
        }
        CheckForInteractables();
    }

    #endregion

    #region Initialization

    private void InitializeComponents()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();

        if (promptObject != null)
        {
            promptText = promptObject.GetComponentInChildren<TextMeshProUGUI>();
            promptObject.SetActive(false);
        }

        if (crosshairImage != null && crosshairImage.canvas != null)
            crosshairImage.canvas.gameObject.SetActive(true);
    }

    private void EnsureCanvasActive()
    {
        if (interactionPromptCanvas != null)
            interactionPromptCanvas.SetActive(true);
    }

    #endregion

    #region Interaction System

    private bool CanInteract()
    {
        EnsureCanvasActive();
        if (Camera.main == null) return false;
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning) return false;
        return true;
    }

private void CheckForInteractables()
{
    Ray ray = GetCenterScreenRay();

    if (TryRaycastInteractable(ray, out RaycastHit hit, out string promptMessage))
    {
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green);
        UpdateUI(true, promptMessage);

        if (Input.GetKeyDown(INTERACT_KEY))
        {
            // If holding broom, let the drop logic below handle E press
            if (!IsHoldingBroom)
                HandleInteraction(hit);
        }
    }
    else
    {
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
        UpdateUI(false, string.Empty);
    }

    if (justPickedUpBroom)
    {
        justPickedUpBroom = false;
        return;
    }

    if (IsHoldingBroom && Input.GetKeyDown(INTERACT_KEY))
    {
        Ray dropRay = GetCenterScreenRay();
        bool hittingDirtSpot = false;

        if (Physics.Raycast(dropRay, out RaycastHit dropHit, interactDistance, ~excludeLayers))
        {
            if (dropHit.collider.GetComponentInParent<DirtSpot>() != null)
            {
                hittingDirtSpot = true;
                dropHit.collider.GetComponentInParent<DirtSpot>().Interact(this);
            }
        }

        if (!hittingDirtSpot)
        {
            Broom broom = currentHeldItem?.GetComponent<Broom>();
            if (broom != null)
                broom.Drop(this);
        }
    }
}
  
    private Ray GetCenterScreenRay()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f);
        return Camera.main.ScreenPointToRay(screenCenter);
    }

    private bool TryRaycastInteractable(Ray ray, out RaycastHit hit, out string promptMessage)
    {
        promptMessage = string.Empty;

        if (!Physics.Raycast(ray, out hit, interactDistance, ~excludeLayers))
        {
            if (IsHoldingBroom)
            {
                promptMessage = "Press E to put down broom";
                return true;
            }
            return false;
        }

        // Broom held — only allow dirt spot interaction
        if (IsHoldingBroom)
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable is DirtSpot)
            {
                promptMessage = interactable.GetInteractionPrompt();
                return !string.IsNullOrEmpty(promptMessage);
            }
            promptMessage = "Press E to put down broom";
            return true;
        }

        // Normal IInteractable check
        IInteractable normalInteractable = hit.collider.GetComponentInParent<IInteractable>();
        if (normalInteractable != null)
        {
            promptMessage = normalInteractable.GetInteractionPrompt();
            return !string.IsNullOrEmpty(promptMessage);
        }

        DrinkMachine machine = hit.collider.GetComponentInParent<DrinkMachine>();
        if (machine != null)
        {
            if (currentHeldItem != null && currentHeldItem.GetComponent<Cup>() != null)
                promptMessage = $"Press E to use {machine.gameObject.name}";
            return currentHeldItem != null && currentHeldItem.GetComponent<Cup>() != null;
        }

        Register register = hit.collider.GetComponentInParent<Register>();
        if (register != null)
        {
            if (currentHeldItem != null)
            {
                Cup heldCup = currentHeldItem.GetComponent<Cup>();
                if (heldCup != null && heldCup.contents != Cup.DrinkType.None)
                    promptMessage = "Press E to use Register";
                return heldCup != null && heldCup.contents != Cup.DrinkType.None;
            }
            return false;
        }

        Trash bin = hit.collider.GetComponentInParent<Trash>();
        if (bin != null)
        {
            if (currentHeldItem != null)
                promptMessage = "Press E to use Bin";
            return currentHeldItem != null;
        }

        Cup cup = hit.collider.GetComponentInParent<Cup>();
        if (cup != null && currentHeldItem == null)
        {
            switch (cup.cupType)
            {
                case Cup.CupType.DraftBeer:    promptMessage = "Press E to pick up Draft Cup"; break;
                case Cup.CupType.TakeawayBeer: promptMessage = "Press E to pick up Takeaway Bottle"; break;
                case Cup.CupType.Spirit:       promptMessage = "Press E to pick up Spirit Cup"; break;
                default:                       promptMessage = "Press E to pick up Cup"; break;
            }
            return true;
        }

        return false;
    }

    private void HandleInteraction(RaycastHit hit)
    {
        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null)
        {
            interactable.Interact(this);
            return;
        }

        DrinkMachine machine = hit.collider.GetComponentInParent<DrinkMachine>();
        if (machine != null)
        {
            machine.Interact(this);
            return;
        }

        Register register = hit.collider.GetComponentInParent<Register>();
        if (register != null)
        {
            register.Interact(this);
            return;
        }

        Trash bin = hit.collider.GetComponentInParent<Trash>();
        if (bin != null)
        {
            bin.Interact(this);
            return;
        }

        Cup cup = hit.collider.GetComponentInParent<Cup>();
        if (cup != null && currentHeldItem == null)
        {
            Debug.Log("[PlayerInteraction] Picking up cup: " + Cup.GetDisplayName(cup.contents));
            cup.OnPickedUp();
            PickUpItem(cup.gameObject);
            return;
        }
    }

    #endregion

    #region UI Updates

    private void UpdateUI(bool isLookingAtInteractable, string message)
    {
        UpdatePromptText(isLookingAtInteractable, message);
        UpdateCrosshair(isLookingAtInteractable);
    }

    private void UpdatePromptText(bool shouldShow, string message)
    {
        if (promptObject == null) return;
        promptObject.SetActive(shouldShow);
        if (shouldShow && promptText != null)
            promptText.text = message;
    }

    private void UpdateCrosshair(bool isInteractable)
    {
        if (crosshairImage == null) return;
        UpdateCrosshairColor(isInteractable);
        UpdateCrosshairScale(isInteractable);
    }

    private void UpdateCrosshairColor(bool isInteractable)
    {
        crosshairImage.color = isInteractable ? interactColor : normalColor;
    }

    private void UpdateCrosshairScale(bool isInteractable)
    {
        float targetScale = CalculateCrosshairScale(isInteractable);
        crosshairImage.transform.localScale = Vector3.one * targetScale;
    }

    private float CalculateCrosshairScale(bool isInteractable)
    {
        if (!isInteractable) return 1.0f;
        float pulseOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
        return interactScale * (1.0f + pulseOffset);
    }

    #endregion

    #region Item Pickup & Holding

    public void PickUpItem(GameObject item)
    {
        currentHeldItem = item;
        originalItemLayer = item.layer;
        ConfigureItemPhysics(item);
        AttachItemToHoldPoint(item);
    }

    private void ConfigureItemPhysics(GameObject item)
    {
        if (item.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = true;
        if (item.TryGetComponent(out Collider col))
            col.isTrigger = true;
    }

    private void SetItemLayer(GameObject item)
    {
        int heldLayer = LayerMask.NameToLayer(HELD_ITEMS_LAYER);
        if (heldLayer != -1)
            SetLayerRecursive(item, heldLayer);
        else
            Debug.LogWarning($"Layer '{HELD_ITEMS_LAYER}' not found.");
    }

    private void AttachItemToHoldPoint(GameObject item)
    {
        item.transform.SetParent(holdPoint);
        item.transform.localPosition = holdPosition;
        item.transform.localRotation = Quaternion.Euler(holdRotation);
    }

    private void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, newLayer);
    }

    public void DropHeldItem()
    {
        if (currentHeldItem == null) return;
        if (currentHeldItem.TryGetComponent(out Rigidbody rb))
            rb.isKinematic = false;
        if (currentHeldItem.TryGetComponent(out Collider col))
            col.isTrigger = false;
        SetLayerRecursive(currentHeldItem, originalItemLayer);
        currentHeldItem.transform.SetParent(null);
        currentHeldItem = null;
    }

    #endregion
}