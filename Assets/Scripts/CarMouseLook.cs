using UnityEngine;
using Yarn.Unity;

public class CarMouseLook : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    [SerializeField] private float sensitivityX = 2f;
    [SerializeField] private float sensitivityY = 2f;

    [Header("Vertical Clamp (degrees)")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Horizontal Clamp (degrees, 0 = unlimited)")]
    [SerializeField] private float maxYaw = 80f;

    private float currentPitch = 0f;
    private float currentYaw = 0f;
    private DialogueRunner dialogueRunner;

    private void Start()
    {
        dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        // Lock cursor here via PauseManager not directly
        if (PauseManager.Instance != null)
            PauseManager.Instance.HideCursorPublic();
    }

    private void OnEnable() { } // Do nothing

    private void OnDisable() { } // Do nothing

    private void Update()
    {
        if (dialogueRunner != null && dialogueRunner.IsDialogueRunning) return;
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused()) return;

        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

        currentYaw += mouseX;
        currentPitch -= mouseY;

        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        if (maxYaw > 0f)
            currentYaw = Mathf.Clamp(currentYaw, -maxYaw, maxYaw);

        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }
}