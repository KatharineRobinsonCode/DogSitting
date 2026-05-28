using UnityEngine;

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

    private void OnEnable()
    {
        Debug.Log("[CarMouseLook] OnEnable fired");
        currentPitch = 0f;
        currentYaw = 0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;
        
        currentYaw   += mouseX;
        currentPitch -= mouseY;

        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
        if (maxYaw > 0f)
            currentYaw = Mathf.Clamp(currentYaw, -maxYaw, maxYaw);

        transform.localRotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }
}