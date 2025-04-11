using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;


/// <summary>
/// 설계도에 배치된 방 정보.
/// </summary>
public class BlueprintRoom : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>RoomData 참조</summary>
    public RoomData bpRoomData;

    /// <summary>선택된 레벨 인덱스 (0~2)</summary>
    public int bpLevelIndex;

    /// <summary>회전 각</summary>
    public int bpRotation;

    /// <summary>배치 위치</summary>
    public Vector2Int bpPosition;

    /// <summary>
    /// 설계도 함선
    /// </summary>
    private BlueprintShip blueprintShip;

    /// <summary>
    /// 드래그 중인지 여부
    /// </summary>
    private bool isDragging = false;

    /// <summary>해당 레벨의 설치 비용</summary>
    public int bpRoomCost;

    /// <summary>해당 레벨의 크기</summary>
    public Vector2Int bpRoomSize;

    /// <summary>
    /// 실제 점유 타일
    /// </summary>
    public List<Vector2Int> occupiedTiles = new();

    private SpriteRenderer sr;
    private Color validColor = new(0f, 1f, 0f, 0.5f);
    private Color invalidColor = new(1f, 0f, 0f, 0.5f);

    private Vector2Int originalPos;
    private int originalRot;
    private GridPlacer gridPlacer;
    RoomData.RoomLevel levelData;

    // collider size 맞춤
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;

        float width = sprite.rect.width / sprite.pixelsPerUnit;
        float height = sprite.rect.height / sprite.pixelsPerUnit;

        GetComponent<BoxCollider2D>().size = new Vector2(width, height);
    }

    private void Update()
    {
        if (!isDragging)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            // 1. 현재 마우스 위치 기준 그리드 좌표 계산
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int hoveredTile = gridPlacer.WorldToGridPosition(mouseWorldPos);

            // 2. 회전 적용
            bpRotation = (bpRotation + 90) % 360;
            bpRoomSize = RoomRotationUtility.GetRotatedSize(levelData.size, bpRotation);

            // 3. 기준 위치 업데이트
            bpPosition = hoveredTile;

            // 4. 오프셋 보정 후 위치 적용
            Vector2 offset = RoomRotationUtility.GetRotationOffset(bpRoomSize, bpRotation);

            transform.position = gridPlacer.GridToWorldPosition(bpPosition) + (Vector3)offset;
            transform.rotation = Quaternion.Euler(0, 0, -bpRotation);

            // 배치 가능 검사
            bool canPlace = gridPlacer.CanPlaceRoom(bpRoomData, bpLevelIndex, bpPosition, bpRotation);
            sr.color = canPlace ? validColor : invalidColor;
        }
    }

    /// <summary>
    /// 설치 시 초기화
    /// </summary>
    public void Initialize(RoomData data, int level, Vector2Int pos, int rot)
    {
        occupiedTiles.Clear();

        bpRoomData = data;
        bpLevelIndex = level;
        bpPosition = pos;
        bpRotation = rot;

        levelData = data.GetRoomDataByLevel(bpLevelIndex);
        bpRoomCost = levelData.cost;
        bpRoomSize = levelData.size;

        bpRoomSize = RoomRotationUtility.GetRotatedSize(bpRoomSize, bpRotation);
        occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpRoomSize, bpRotation);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = levelData.roomSprite;

        transform.rotation = Quaternion.Euler(0, 0, -bpRotation);
    }

    public void SetBlueprint(BlueprintShip bpShip)
    {
        blueprintShip = bpShip;
    }

    public BlueprintShip GetBlueprintShip()
    {
        return blueprintShip;
    }

    public void SetGridPlacer(GridPlacer placer)
    {
        gridPlacer = placer;
    }

    /// <summary>
    /// 클릭 후 방 삭제 여부 검토
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            RoomSelectionHandler.Instance.SelectRoom(this);
    }

    // 여기부터 추가
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        originalPos = bpPosition;
        originalRot = bpRotation;

        RoomSelectionHandler.Instance.Deselect(); // 드래그 전 선택 해제
        gridPlacer.UnMarkRoomOccupied(this); // 점유 타일 없앰
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int newGrid = gridPlacer.WorldToGridPosition(mouseWorld);

        bpPosition = newGrid;
        Vector2 offset = RoomRotationUtility.GetRotationOffset(bpRoomSize, bpRotation);
        transform.position = gridPlacer.GridToWorldPosition(bpPosition) + (Vector3)offset;

        bool canPlace = gridPlacer.CanPlaceRoom(bpRoomData, bpLevelIndex, bpPosition, bpRotation);
        sr.color = canPlace ? validColor : invalidColor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int newPos = gridPlacer.WorldToGridPosition(mouseWorld);

        bool canPlace = gridPlacer.CanPlaceRoom(bpRoomData, bpLevelIndex, newPos, bpRotation);

        // 유효성 검사
        if (!canPlace)
        {
            // 불가능 : 원위치
            bpPosition = originalPos;
            bpRotation = originalRot;
            bpRoomSize = RoomRotationUtility.GetRotatedSize(levelData.size, bpRotation);

            Vector2 offset = RoomRotationUtility.GetRotationOffset(bpRoomSize, bpRotation);
            transform.position = gridPlacer.GridToWorldPosition(originalPos) + (Vector3)offset;
            transform.rotation = Quaternion.Euler(0, 0, -bpRotation);
        }
        else
        {
            bpPosition = newPos;
            occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpRoomSize, bpRotation);
            gridPlacer.MarkRoomOccupied(this);

            // 설치 확정
            RoomSelectionHandler.Instance.SelectRoom(this);
        }
        sr.color = Color.white;
    }
}
