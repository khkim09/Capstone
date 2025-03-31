using UnityEngine;

/// <summary>
/// 함선 커스터마이징 시 나타날 그리드 관련 매니저입니다.
/// </summary>
public class ShipGridManager : MonoBehaviour
{
    public static ShipGridManager Instance;

    /// <summary>
    /// 그리드 총 높이, 넓이 (60 x 60)
    /// tile 1개 사이즈 (1)
    /// 그리드 중심 (-30, 30)
    /// </summary>
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

    /// <summary>
    /// ShipGridManager.cs가 호출됨과 동시에 그리드 중심 다시 계산
    /// </summary>
    private void Start()
    {
        gridOrigin = new Vector2(-gridWidth / 2f * tileSize, -gridHeight / 2f * tileSize);
    }

    /// <summary>
    /// 그리드에서 선택한 좌표를 world 좌표로 변환합니다.
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector2 GridToWorldPosition(Vector2Int gridPos)
    {
        return gridOrigin + new Vector2((gridPos.x + 0.5f) * tileSize, (gridPos.y + 0.5f) * tileSize);
    }

    /// <summary>
    /// world 좌표를 그리드에서의 좌표로 변환합니다.
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        int x = (int)(Mathf.FloorToInt(worldPos.x / tileSize) - gridOrigin.x);
        int y = (int)(Mathf.FloorToInt(worldPos.y / tileSize) - gridOrigin.y);
        return new Vector2Int(x, y);
    }
}
