using UnityEngine;

/// <summary>
/// 방 인벤토리 항목을 드래그하여 설계도에 배치할 수 있게 하는 시스템.
/// 프리뷰 표시 및 설치 타이밍 조정 포함.
/// </summary>
public class BlueprintRoomDragHandler : MonoBehaviour
{
    [Header("Referecnes")]
    public GameObject previewPrefab;
    public GridPlacer gridPlacer;

    [Header("preview sprite color")]
    public Color validColor = new(0, 1, 0, 0.5f);
    public Color invalidColor = new(1, 0, 0, 0.5f);

    private GameObject previewGO;
    private SpriteRenderer previewRenderer;

    private RoomData currentRoomData;
    private int currentLevel;
    private int currentRotation; // 0, 90, 180, 270
    private bool isDragging = false;
    private Vector2Int roomSize;

    /// <summary>
    /// 드래그 시작 시 호출됨.
    /// </summary>
    public void StartDragging(RoomData data, int level)
    {
        if (previewGO != null)
            Destroy(previewGO);

        currentRoomData = data;
        currentLevel = level;
        currentRotation = 0;
        isDragging = true;

        RoomData.RoomLevel levelData = data.GetRoomData(level);
        roomSize = levelData.size;

        previewGO = Instantiate(previewPrefab);
        previewRenderer = previewGO.GetComponent<SpriteRenderer>();
        previewRenderer.sprite = levelData.roomSprite;
        previewRenderer.color = validColor;

        previewGO.transform.localScale = Vector3.one;
        previewGO.transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!isDragging || currentRoomData == null || previewGO == null)
            return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = gridPlacer.WorldToGridPosition(mouseWorld);
        Vector3 basePos = gridPlacer.GridToWorldPosition(gridPos);

        Vector2Int rotatedSize = RoomRotationUtility.GetRotatedSize(roomSize, currentRotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(rotatedSize, currentRotation);

        // 좌측 하단 블록 기준 설치
        previewGO.transform.position = basePos + (Vector3)offset;
        previewGO.transform.rotation = Quaternion.Euler(0, 0, -currentRotation);

        // 설치 가능 여부 시각화
        bool canPlace = gridPlacer.CanPlaceRoom(currentRoomData, currentLevel, gridPos, currentRotation);
        previewRenderer.color = canPlace ? validColor : invalidColor;

        // 회전
        if (Input.GetMouseButtonDown(1))
            currentRotation = (currentRotation + 90) % 360;

        // 설치
        if (Input.GetMouseButtonUp(0) && canPlace)
        {
            gridPlacer.PlaceRoom(currentRoomData, currentLevel, gridPos, currentRotation);

            Destroy(previewGO);
            isDragging = false;
        }
    }
}
