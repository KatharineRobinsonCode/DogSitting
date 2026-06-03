using UnityEngine;

public class StoryFlags : MonoBehaviour
{
    public static StoryFlags Instance { get; private set; }

    // Add more flags as the story grows
    public bool TalkedToNeighbour { get; private set; }
    public bool HasKnife { get; private set; }
    public bool UsedSqueakyToy { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetTalkedToNeighbour() => TalkedToNeighbour = true;
    public void SetHasKnife() => HasKnife = true;
    public void SetUsedSqueakyToy() => UsedSqueakyToy = true;
}