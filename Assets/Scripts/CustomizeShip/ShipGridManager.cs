using UnityEngine;

/// <summary>
/// 함선 커스터마이징 시 사용되는 그리드 시스템을 관리하는 매니저.
/// 그리드 생성, 중심 위치 계산, 좌표 변환 기능을 제공합니다.
/// </summary>
public class ShipGridManager : MonoBehaviour
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static ShipGridManager Instance;

    /// <summary>
    /// 그리드의 가로 칸 수입니다. (기본값: 60)
    /// </summary>
    public int gridWidth = 60;

    /// <summary>
    /// 그리드의 세로 칸 수입니다. (기본값: 60)
    /// </summary>
    public int gridHeight = 60;

    /// <summary>
    /// 타일 하나의 실제 크기입니다. (기본값: 1)
    /// </summary>
    public float tileSize = 1f;

    /// <summary>
    /// 그리드의 좌측 하단 기준 중심 위치입니다. (-30, -30 기준)
    /// </summary>
    public Vector2 gridOrigin = new Vector2(-30, -30);

    /// <summary>
    /// 인스턴스를 초기화합니다. 중복 객체는 제거됩니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 시작 시 그리드 중심 위치를 그리드 크기와 타일 크기에 따라 다시 계산합니다.
    /// </summary>
    private void Start()
    {
        gridOrigin = new Vector2(-gridWidth / 2f * tileSize, -gridHeight / 2f * tileSize);
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="gridPos">변환할 그리드 좌표.</param>
    /// <returns>해당 위치의 월드 좌표.</returns>
    public Vector2 GridToWorldPosition(Vector2Int gridPos)
    {
        return gridOrigin + new Vector2((gridPos.x + 0.5f) * tileSize, (gridPos.y + 0.5f) * tileSize);
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    /// <param name="worldPos">변환할 월드 좌표.</param>
    /// <returns>해당 위치의 그리드 좌표.</returns>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        int x = (int)(Mathf.FloorToInt(worldPos.x / tileSize) - gridOrigin.x);
        int y = (int)(Mathf.FloorToInt(worldPos.y / tileSize) - gridOrigin.y);
        return new Vector2Int(x, y);
    }
}
