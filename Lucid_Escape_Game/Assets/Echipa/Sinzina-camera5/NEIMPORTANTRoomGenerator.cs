using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Prefab-uri structura camera")]
    public GameObject[] wallPrefabs;
    public GameObject floorPrefab;
    public GameObject ceilingPrefab;

    [Header("Inaltime pereti")]
    public float wallHeight = 3f;

    private void Start()
    {
        GenerateRoom();
    }

    void GenerateRoom()
    {
        Vector3 origin = transform.position;

        // 1. Punem podeaua la scale natural (1,1,1) - fara sa o marim/micsoram
        GameObject floor = Instantiate(floorPrefab, origin, Quaternion.identity, transform);

        // 2. Citim cat de mare e REALMENTE podeaua, asa cum a facut-o artistul
        Vector3 floorSize = GetSize(floor);
        float roomWidth = floorSize.x;
        float roomDepth = floorSize.z;

        // 3. Plafonul, la aceeasi dimensiune ca podeaua
        if (ceilingPrefab != null)
        {
            Vector3 ceilingPos = origin + new Vector3(0, wallHeight, 0);
            Instantiate(ceilingPrefab, ceilingPos, Quaternion.identity, transform);
        }

        // 4. Peretii, calculati exact pe marginea podelei reale
        float wallLength = GetSize(wallPrefabs[0]).x;

        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;

        BuildWallLine(origin + new Vector3(-halfW, 0, -halfD), Vector3.right, roomWidth, 0f, wallLength);
        BuildWallLine(origin + new Vector3(-halfW, 0, halfD), Vector3.right, roomWidth, 0f, wallLength);
        BuildWallLine(origin + new Vector3(-halfW, 0, -halfD), Vector3.forward, roomDepth, 90f, wallLength);
        BuildWallLine(origin + new Vector3(halfW, 0, -halfD), Vector3.forward, roomDepth, 90f, wallLength);
    }

    // Citeste dimensiunea reala (world size) a unui obiect
    Vector3 GetSize(GameObject obj)
    {
        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend == null) return Vector3.one;
        return rend.bounds.size;
    }

    void BuildWallLine(Vector3 start, Vector3 direction, float totalLength, float yRotation, float wallLength)
    {
        int segments = Mathf.CeilToInt(totalLength / wallLength);
        float actualSegmentLength = totalLength / segments;

        for (int i = 0; i < segments; i++)
        {
            Vector3 pos = start + direction * (i * actualSegmentLength + actualSegmentLength / 2f);
            GameObject prefab = wallPrefabs[Random.Range(0, wallPrefabs.Length)];
            Instantiate(prefab, pos, Quaternion.Euler(0, yRotation, 0), transform);
        }
    }
}