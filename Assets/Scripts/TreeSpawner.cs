using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject[] treePrefabs;
    public int treesPerSide = 50;
    public float roadLength = 300f;
    public float roadStartZ = 0f;
    public float distanceFromRoad = 8f;
    public float minScale = 0.8f;
    public float maxScale = 1.4f;
    public float randomOffset = 3f;

    [ContextMenu("Spawn Trees")]
    public void SpawnTrees()
    {
        // Clear existing children
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        for (int i = 0; i < treesPerSide; i++)
        {
            float zPos = roadStartZ + (roadLength / treesPerSide) * i;
            float randomZ = Random.Range(-randomOffset, randomOffset);
            float randomX = Random.Range(0f, randomOffset);
            float scale = Random.Range(minScale, maxScale);

            // Left side
            SpawnTree(
                new Vector3(-distanceFromRoad - randomX, 0, zPos + randomZ),
                scale
            );

            // Right side
            SpawnTree(
                new Vector3(distanceFromRoad + randomX, 0, zPos + randomZ),
                scale
            );
        }

        Debug.Log($"[TreeSpawner] Spawned {treesPerSide * 2} trees");
    }

    void SpawnTree(Vector3 position, float scale)
    {
        if (treePrefabs.Length == 0) return;

        GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        GameObject tree = Instantiate(prefab, position, 
            Quaternion.Euler(0, Random.Range(0f, 360f), 0), transform);
        tree.transform.localScale = Vector3.one * scale;
    }
}