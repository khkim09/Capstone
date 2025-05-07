using UnityEngine;

/// <summary>
/// 방 인벤토리 항목을 드래그하여 설계도에 배치할 수 있게 하는 시스템.
/// 프리뷰 표시 및 설치 타이밍 조정 포함.
/// </summary>
public class BlueprintRoomDragHandler : MonoBehaviour
{
    [Header("References")] public GameObject previewPrefab;
    public GridPlacer gridPlacer;

    [Header("preview sprite color")] public Color validColor = new(0, 1, 0, 0.5f);
    public Color invalidColor = new(1, 0, 0, 0.5f);

    private GameObject previewGO;
    private SpriteRenderer previewRenderer;

    private RoomData draggingRoomData;
    private int draggingLevel;
    private RotationConstants.Rotation draggingRotation;

    private bool isDragging = false;
    private Vector2Int roomSize;

    // 이전 정적 변수 대신 BlueprintDragManager 참조를 통해 드래그 상태 확인
    public static bool IsRoomBeingDragged =>
        BlueprintDragManager.Instance != null && BlueprintDragManager.Instance.IsRoomBeingDragged;

    /// <summary>
    /// 드래그 시작 시 호출됨.
    /// </summary>
    public void StartDragging(RoomData data, int level)
    {
        // 이미 다른 드래그가 진행 중이면 무시
        if (!BlueprintDragManager.Instance.StartRoomDrag())
            return;

        if (previewGO != null)
            Destroy(previewGO);

        draggingRoomData = data;
        draggingLevel = level;
        draggingRotation = RotationConstants.Rotation.Rotation0;
        isDragging = true;

        RoomData.RoomLevel levelData = data.GetRoomDataByLevel(level);
        roomSize = levelData.size;

        previewGO = Instantiate(previewPrefab);
        previewRenderer = previewGO.GetComponent<SpriteRenderer>();
        previewRenderer.sprite = levelData.roomSprite;
        previewRenderer.color = validColor;

        previewGO.transform.localScale = Vector3.one;
        previewGO.transform.rotation = Quaternion.identity;
    }

    /// <summary>
    /// 드래그 중단
    /// </summary>
    public void StopDragging()
    {
        if (previewGO != null)
            Destroy(previewGO);

        previewGO = null;
        draggingRoomData = null;
        isDragging = false;

        // 드래그 상태 해제
        BlueprintDragManager.Instance.StopDrag();
    }

    private void Update()
    {
        if (!isDragging || draggingRoomData == null || previewGO == null)
            return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = gridPlacer.WorldToGridPosition(mouseWorld);
        Vector3 basePos = gridPlacer.GridToWorldPosition(gridPos);

        Vector2Int rotatedSize = RoomRotationUtility.GetRotatedSize(roomSize, draggingRotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(rotatedSize, draggingRotation);

        // 좌측 하단 블록 기준 설치
        previewGO.transform.position = basePos + (Vector3)offset;
        previewGO.transform.rotation = Quaternion.Euler(0, 0, -(int)draggingRotation * 90);

        // 설치 가능 여부 시각화
        bool canPlace = gridPlacer.CanPlaceRoom(draggingRoomData, draggingLevel, gridPos, draggingRotation);
        previewRenderer.color = canPlace ? validColor : invalidColor;

        // 회전
        if (isDragging && Input.GetMouseButtonDown(1))
            draggingRotation = (RotationConstants.Rotation)(((int)draggingRotation + 1) % 4);

        // 설치
        if (Input.GetMouseButtonUp(0))
        {
            if (!canPlace)
            {
                StopDragging();
            }
            else
            {
                gridPlacer.PlaceRoom(draggingRoomData, draggingLevel, gridPos, draggingRotation);
                StopDragging();
            }
        }
    }
}
