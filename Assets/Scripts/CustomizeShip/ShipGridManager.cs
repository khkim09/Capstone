using UnityEngine;

public class ShipGridManager : MonoBehaviour
{
    public static ShipGridManager Instance;

    public int gridWidth = 60;
    public int gridHeight = 60;
    public float tileSize = 1f;
    public Vector2 gridOrigin = new Vector2(-30, -30);

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
        return gridOrigin + new Vector2((gridPos.x + 0.5f) * tileSize, (gridPos.y + 0.5f) * tileSize);
    }

    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        int x = (int)(Mathf.FloorToInt(worldPos.x / tileSize) - gridOrigin.x);
        int y = (int)(Mathf.FloorToInt(worldPos.y / tileSize) - gridOrigin.y);
        return new Vector2Int(x, y);
    }
}
