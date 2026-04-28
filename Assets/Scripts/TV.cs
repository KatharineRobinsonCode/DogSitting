using UnityEngine;

public class TV : MonoBehaviour, IInteractable
{
    [Header("TV Settings")]
    [SerializeField] private GameObject tvStaticImage;
    [SerializeField] private AudioSource tvAudio;

    private bool isOn = false;

    public string GetInteractionPrompt()
    {
        return isOn ? "Press E to turn off TV" : "Press E to turn on TV";
    }

    public void Interact(PlayerInteraction player)
    {
        isOn = !isOn;

        if (isOn)
            TurnOn();
        else
            TurnOff();
    }

    private void TurnOn()
    {
        if (tvStaticImage != null)
            tvStaticImage.SetActive(true);

        if (tvAudio != null)
            tvAudio.Play();
    }

    private void TurnOff()
    {
        if (tvStaticImage != null)
            tvStaticImage.SetActive(false);

        if (tvAudio != null)
            tvAudio.Stop();
    }
}