using UnityEngine;

public class ShipGridManager : MonoBehaviour
{
    public static ShipGridManager Instance;

    public int gridWidth = 20; // 함선 커스터마이징 가능 영역
    public int gridHeight = 20;
    public float tileSize = 0.5f;
    public Vector2 gridOrigin = new Vector2(-10, -10);

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gridOrigin = new Vector2(-gridWidth / 2f * tileSize, -gridHeight / 2f * tileSize);
    }

    public Vector2 GridToWorldPosition(Vector2Int gridPos)
    {
        return gridOrigin + new Vector2(gridPos.x * tileSize, gridPos.y * tileSize);
    }

    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector2 local = worldPos - gridOrigin;
        return new Vector2Int(Mathf.FloorToInt(local.x / tileSize), Mathf.FloorToInt(local.y / tileSize));
    }

    public bool IsWithinBounds(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth && gridPos.y >= 0 && gridPos.y < gridHeight;
    }
}
