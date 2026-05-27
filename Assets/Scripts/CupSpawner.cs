using UnityEngine;
using System.Collections;

public class CupSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cupPrefab;
    [SerializeField] private float respawnDelay = 3f;

    private void Start()
    {
        SpawnCup();
    }

    public void OnCupTaken()
    {
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnCup();
    }

    private void SpawnCup()
    {
        if (cupPrefab != null)
            Instantiate(cupPrefab, transform.position, transform.rotation, transform);
    }
}