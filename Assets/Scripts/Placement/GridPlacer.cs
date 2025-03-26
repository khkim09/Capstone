using System.Collections.Generic;
using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    public static GridPlacer Instance;

    public GameObject roomPrefab;

    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();

    private void Awake()
    {
        Instance = this;
    }

    public bool CanPlaceRoom(Vector2Int startGrid, Vector2Int roomSize)
    {
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int checkPos = startGrid + new Vector2Int(x, y);
                if (occupiedTiles.Contains(checkPos))
                    return false;
            }
        }
        return true;
    }

    public void PlaceRoom(Vector2Int startGrid, Vector2Int roomSize)
    {
        if (!CanPlaceRoom(startGrid, roomSize))
        {
            Debug.LogWarning("설치 실패: 공간이 이미 점유되어 있음");
            return;
        }

        GameObject roomObj = Instantiate(roomPrefab);
        Vector2 centerOffset = new Vector2((roomSize.x - 1) * 0.5f, (roomSize.y - 1) * 0.5f);
        roomObj.transform.position = ShipGridManager.Instance.GridToWorldPosition(startGrid) + centerOffset;

        // 점유 타일 등록
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                occupiedTiles.Add(startGrid + new Vector2Int(x, y));
            }
        }
    }
}
