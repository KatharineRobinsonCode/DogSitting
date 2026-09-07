using UnityEngine;

public class StoryFlags : MonoBehaviour
{
    public static StoryFlags Instance { get; private set; }

   public int BeerMatFlipScore { get; private set; }
public int BeerMatStackScore { get; private set; }

public void SetBeerMatFlipScore(int score) => BeerMatFlipScore = score;
public void SetBeerMatStackScore(int score) => BeerMatStackScore = score;

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