using UnityEngine;

public class TileVisualRenderer : MonoBehaviour
{
    public GameObject tileVisualPrefab;

    private void Start()
    {
        for (int x = 0; x < ShipGridManager.Instance.gridWidth; x++)
        {
            for (int y = 0; y < ShipGridManager.Instance.gridHeight; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector2 worldPos = ShipGridManager.Instance.GridToWorldPosition(gridPos);
                Instantiate(tileVisualPrefab, worldPos, Quaternion.identity, transform);
            }
        }
    }
}
