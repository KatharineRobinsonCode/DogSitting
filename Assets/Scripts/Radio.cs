using UnityEngine;

public class Radio : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip radioOnClip;
    [SerializeField] private AudioClip radioOffClip;

    [Header("Settings")]
    [SerializeField] private bool startsOn = false;

    private bool isOn;

    void Start()
    {
        isOn = startsOn;

        if (audioSource != null && startsOn && radioOnClip != null)
        {
            audioSource.clip = radioOnClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public string GetInteractionPrompt()
    {
        return isOn ? "Press E to turn off radio" : "Press E to turn on radio";
    }

    public void Interact(PlayerInteraction player)
    {
        isOn = !isOn;

        if (isOn)
            TurnOn();
        else
            TurnOff();
    }

    void TurnOn()
    {
        Debug.Log("[Radio] Turned on");

        if (audioSource != null && radioOnClip != null)
        {
            audioSource.clip = radioOnClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        FeedbackManager.Instance?.ShowMessage(
            "Radio on", 
            FeedbackManager.MessageType.Info
        );
    }

void TurnOff()
{
    Debug.Log("[Radio] Turned off");

    if (audioSource != null)
    {
        audioSource.Stop(); // stop looping music first
        
        if (radioOffClip != null)
            audioSource.PlayOneShot(radioOffClip); // now plays uninterrupted
    }

    FeedbackManager.Instance?.ShowMessage("Radio off", FeedbackManager.MessageType.Info);
}
}