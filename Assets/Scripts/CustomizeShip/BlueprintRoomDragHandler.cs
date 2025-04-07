using UnityEngine;

/// <summary>
/// 방 인벤토리 항목을 드래그하여 설계도에 배치할 수 있게 하는 시스템.
/// 프리뷰 표시 및 설치 타이밍 조정 포함.
/// </summary>
public class BlueprintRoomDragHandler : MonoBehaviour
{
    [Header("방 프리뷰로 사용할 프리팹 (반투명 SpriteRenderer 포함)")]
    public GameObject previewPrefab;

    [Header("배치 유효성을 검사하고 실제 배치하는 GridPlacer 참조")]
    public GridPlacer gridPlacer;

    [Header("preview sprite color")]
    public Color validColor = new(0, 1, 0, 0.5f);
    public Color invalidColor = new(1, 0, 0, 0.5f);

    private GameObject previewGO;
    private SpriteRenderer previewRenderer;

    private RoomData currentRoomData;
    private int currentLevel;
    private int currentRotation; // 0, 90, 180, 270
    private Vector2Int currentSize;
    private bool isDragging = false;

    /// <summary>
    /// 드래그 시작 시 호출됨.
    /// </summary>
    public void StartDragging(RoomData data, int level)
    {
        currentRoomData = data;
        currentLevel = level;
        currentRotation = 0;
        isDragging = true;

        RoomData.RoomLevel levelData = data.GetRoomData(level);
        currentSize = levelData.size;

        previewGO = Instantiate(previewPrefab);
        previewRenderer = previewGO.GetComponent<SpriteRenderer>();
        previewRenderer.sprite = levelData.roomSprite;
        previewRenderer.color = validColor;

        previewGO.transform.localScale = new Vector3(currentSize.x, currentSize.y, 1);
    }

    private void Update()
    {
        if (!isDragging || currentRoomData == null || previewGO == null)
            return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = new(Mathf.FloorToInt(mouseWorld.x), Mathf.FloorToInt(mouseWorld.y));

        // 좌측 하단 블록 기준 설치
        float dx = currentSize.x / 2.0f - 0.5f;
        float dy = currentSize.y / 2.0f - 0.5f;
        previewGO.transform.position = new Vector3(gridPos.x + dx, gridPos.y + dy, 0);
        previewGO.transform.rotation = Quaternion.Euler(0, 0, -currentRotation);

        // 회전
        if (Input.GetMouseButtonDown(1))
        {
            currentRotation = (currentRotation + 90) % 360;
            previewGO.transform.rotation = Quaternion.Euler(0, 0, -currentRotation);
        }

        // 설치 가능 여부 시각화
        bool canPlace = gridPlacer.CanPlaceRoom(currentRoomData, currentLevel, gridPos, currentRotation);
        previewRenderer.color = canPlace ? validColor : invalidColor;

        // 설치
        if (Input.GetMouseButtonUp(0) && canPlace)
        {
            gridPlacer.PlaceRoom(currentRoomData, currentLevel, gridPos, currentRotation);

            Destroy(previewGO);
            isDragging = false;
        }
    }
}
