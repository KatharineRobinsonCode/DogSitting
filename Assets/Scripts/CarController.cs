using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float brakeStrength = 8f;
    [SerializeField] private float turnSpeed = 60f;
    [SerializeField] private float cameraSwayAmount = 2f;

    [Header("Camera Sway")]
    [SerializeField] private Transform cameraRig;

    [Header("Audio")]
    [SerializeField] private AudioSource engineAudio;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.4f;

[Header("Camera Look")]
[SerializeField] private CarMouseLook carMouseLook; // drag cameraRig here in Inspector
    private float currentSpeed = 0f;
    private bool controlsEnabled = false;
    private bool isStopped = false;

    void Update()
    {
        if (!controlsEnabled || isStopped) return;

        HandleAcceleration();
        HandleSteering();
        HandleCameraSway();
        UpdateEngineSound();
    }

    void HandleAcceleration()
    {
        float input = Input.GetAxis("Vertical"); // W/S

        if (input > 0)
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        else if (input < 0)
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeStrength * Time.deltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, acceleration * 0.5f * Time.deltaTime);

        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

    void HandleSteering()
    {
        float input = Input.GetAxis("Horizontal"); // A/D
        if (currentSpeed > 0.1f)
        {
            float turn = input * turnSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, turn);
        }
    }

    void HandleCameraSway()
    {
        if (cameraRig == null) return;
        float input = Input.GetAxis("Horizontal");
        float targetZ = -input * cameraSwayAmount;
        Vector3 current = cameraRig.localEulerAngles;
        cameraRig.localEulerAngles = new Vector3(
            current.x,
            current.y,
            Mathf.LerpAngle(current.z, targetZ, Time.deltaTime * 3f)
        );
    }

    void UpdateEngineSound()
    {
        if (engineAudio == null) return;
        engineAudio.pitch = Mathf.Lerp(minPitch, maxPitch, currentSpeed / maxSpeed);
    }

 public void EnableControls()
{
    controlsEnabled = true;
    if (engineAudio != null && !engineAudio.isPlaying)
        engineAudio.Play();
    if (carMouseLook != null) carMouseLook.enabled = true;
}

public void DisableControls()
{
    controlsEnabled = false;
    if (carMouseLook != null) carMouseLook.enabled = false;
}

public void StopCar()
{
    isStopped = true;
    currentSpeed = 0f;
    if (engineAudio != null) engineAudio.Stop();
    // Don't deactivate mouse look here — player may still want to look around
    // while parked and interact with the radio
}
    public float CurrentSpeed => currentSpeed;

    public void ResumeCar()
{
    isStopped = false;
    currentSpeed = 0f;
}
}