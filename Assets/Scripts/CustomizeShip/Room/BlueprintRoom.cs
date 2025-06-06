using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 설계도에 배치된 방 정보.
/// </summary>
public class BlueprintRoom : MonoBehaviour, IBlueprintPlaceable
{
    /// <summary>RoomData 참조</summary>
    public RoomData bpRoomData;

    /// <summary>선택된 레벨 인덱스 (0~2)</summary>
    public int bpLevelIndex;

    /// <summary>회전 각</summary>
    public Constants.Rotations.Rotation bpRotation;

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

    /// <summary>
    /// 설계도 방의 Sprite Renderer
    /// </summary>
    private SpriteRenderer sr;

    /// <summary>
    /// 배치 가능을 나타내는 초록색 (alpha = 0.5)
    /// </summary>
    private Color validColor = new(0f, 1f, 0f, 0.5f);

    /// <summary>
    /// 배치 불가능을 나타내는 빨간색 (alpha = 0.5)
    /// </summary>
    private Color invalidColor = new(1f, 0f, 0f, 0.5f);

    /// <summary>
    /// 이동 전 방 위치
    /// </summary>
    private Vector2Int originalPos;

    /// <summary>
    /// 이동 전 방 회전각
    /// </summary>
    private Constants.Rotations.Rotation originalRot;

    /// <summary>
    /// 그리드 타일 배치 작업을 위한 오브젝트
    /// </summary>
    private GridPlacer gridPlacer;

    /// <summary>
    /// 방 레벨 데이터
    /// </summary>
    private RoomData.RoomLevel levelData;

    /// <summary>
    /// 마우스로 클릭한 오브젝트 (방 삭제 여부 활성화 조건을 위한 변수)
    /// </summary>
    private GameObject mouseDownTarget;

    /// <summary>
    /// collider size 맞춤
    /// </summary>
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;

        float width = sprite.rect.width / sprite.pixelsPerUnit;
        float height = sprite.rect.height / sprite.pixelsPerUnit;

        GetComponent<BoxCollider2D>().size = new Vector2(width, height);
    }

    /// <summary>
    /// 방 배치와 관련된 모든 작업을 매 프레임 검사
    /// </summary>
    private void Update()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // 회전
        if (isDragging && Input.GetMouseButtonDown(1))
        {
            // 드래그 중이면 회전 방어 조건
            if (BlueprintRoomDragHandler.IsRoomBeingDragged)
                return;

            if (hit.collider != null && hit.collider.gameObject == gameObject)
                Rotate((Constants.Rotations.Rotation)(((int)bpRotation + 1) % 4));
        }

        // 드래그 시작 (좌클릭)
        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            if (BlockBPMovement())
            {
                isDragging = false;
                return;
            }

            // UI 위에 마우스가 있는지 확인하는 개선된 방법
            if (IsPointerOverBlockingUI())
                return;

            if (EventSystem.current.IsPointerOverGameObject())
                if (IsPointerOverUIObject(RoomSelectionHandler.Instance.selectionUI))
                    return;

            if (RoomSelectionHandler.Instance != null)
                RoomSelectionHandler.Instance.Deselect();

            // 드래그 전 선택된 방 해제
            RoomSelectionHandler.Instance.Deselect();

            // 드래그 시작 시 정보 저장
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                originalPos = bpPosition;
                originalRot = bpRotation;

                gridPlacer.UnMarkObjectOccupied(this);
                mouseDownTarget = hit.collider.gameObject;
            }
            else
            {
                mouseDownTarget = null;
            }
        }

        // 드래그 중
        if (isDragging && Input.GetMouseButton(0))
        {
            if (BlockBPMovement())
            {
                isDragging = false;
                return;
            }

            Vector2Int newPos = gridPlacer.WorldToGridPosition(mouseWorldPos);
            bpPosition = newPos;

            Vector2 offset = RoomRotationUtility.GetRotationOffset(bpRoomSize, bpRotation);
            transform.position = gridPlacer.GetWorldPositionFromGrid(bpPosition) + (Vector3)offset;

            bool canPlace = gridPlacer.CanPlaceRoom(bpRoomData, bpLevelIndex, bpPosition, bpRotation);
            sr.color = canPlace ? validColor : invalidColor;
        }

        // 드래그 종료
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Vector2Int newPos = gridPlacer.WorldToGridPosition(mouseWorldPos);

            UpdateOccupiedTiles();

            bool canPlace = gridPlacer.CanPlaceRoom(bpRoomData, bpLevelIndex, newPos, bpRotation);
            sr.color = Color.white;

            // 유효성 검사
            if (!canPlace)
            {
                // 불가능 : 원위치
                bpPosition = originalPos;
                bpRotation = originalRot;
                bpRoomSize = RoomRotationUtility.GetRotatedSize(levelData.size, bpRotation);

                UpdateOccupiedTiles();

                Vector2 offset = RoomRotationUtility.GetRotationOffset(bpRoomSize, bpRotation);
                transform.position = gridPlacer.GetWorldPositionFromGrid(originalPos) + (Vector3)offset;
                transform.rotation = Quaternion.Euler(0, 0, -(int)bpRotation * 90);

                // 점유 타일 복구
                gridPlacer.MarkObjectOccupied(this);
                mouseDownTarget = null;
            }
            else
            {
                bpPosition = newPos;
                occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpRoomSize, bpRotation);
                gridPlacer.MarkObjectOccupied(this);
            }

            transform.position += new Vector3(0, 0, 10f); // 뒤쪽으로 배치

            if (hit.collider != null && hit.collider.gameObject == mouseDownTarget)
                RoomSelectionHandler.Instance.SelectRoom(this);

            mouseDownTarget = null;
        }
    }

    private bool BlockBPMovement()
    {
        // RTS 선택도 막아야돼서 통일시켜 쓰겠음
        GameObject blockPanel = GameObject.FindWithTag("BlockRTSNBPMove");
        if (blockPanel != null && blockPanel.activeInHierarchy)
            return true;
        return false;
    }

    /// <summary>
    /// 특정 UI 요소들 위에 마우스가 있는지 확인
    /// 특히 인벤토리 UI와 같은 방해가 되는 UI만 체크
    /// </summary>
    private bool IsPointerOverBlockingUI()
    {
        PointerEventData eventData = new(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
            if (
                result.gameObject.name.Contains("Scroll View") || result.gameObject.name.Contains("Essential") ||
                result.gameObject.name.Contains("Auxiliary") || result.gameObject.name.Contains("Living") ||
                result.gameObject.name.Contains("Storage") || result.gameObject.name.Contains("Etc") ||
                result.gameObject.name.Contains("Weapon") || result.gameObject.name.Contains("Hull") ||
                result.gameObject.name.Contains("ControlTab") || result.gameObject.name.Contains("ClearButton") ||
                result.gameObject.name.Contains("SaveButton") || result.gameObject.name.Contains("BPCostText") ||
                result.gameObject.name.Contains("FeedBackText") || result.gameObject.name.Contains("BackButton") ||
                result.gameObject.name.Contains("RoomButtonPrefab")
                )
                return true;

        return false;
    }

    /// <summary>
    /// 설치 시 초기화
    /// </summary>
    public void Initialize(RoomData data, int level, Vector2Int pos, Constants.Rotations.Rotation rot)
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

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = levelData.roomSprite;

        transform.rotation = Quaternion.Euler(0, 0, -(int)bpRotation * 90);
    }

    /// <summary>
    /// 드래그 모드 설정 (BlueprintRoom은 드래그 모드에서 시각적 변경만 적용)
    /// IDraggableItem 인터페이스 구현
    /// </summary>
    public void SetDragMode(bool isDragging)
    {
        // 드래그 중일 때 반투명하게 처리
        if (sr != null)
        {
            if (isDragging)
                // 드래그 모드 활성화 시 반투명하게
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0.5f);
            else
                // 드래그 모드 비활성화 시 원래 색상으로
                sr.color = Color.white;
        }
    }

    /// <summary>
    /// 오브젝트 회전
    /// </summary>
    public void Rotate(Constants.Rotations.Rotation rotation)
    {
        bpRotation = rotation;
        bpRoomSize = RoomRotationUtility.GetRotatedSize(levelData.size, bpRotation);
        transform.rotation = Quaternion.Euler(0, 0, -(int)bpRotation * 90);
    }

    /// <summary>
    /// 현재 회전 상태 반환
    /// </summary>
    public object GetRotation()
    {
        return bpRotation;
    }

    /// <summary>
    /// 현재 그리드 위치 반환
    /// </summary>
    public Vector2Int GetGridPosition()
    {
        return bpPosition;
    }

    /// <summary>
    /// 그리드 위치 설정
    /// </summary>
    public void SetGridPosition(Vector2Int position)
    {
        bpPosition = position;
    }

    /// <summary>
    /// 설계도 함선 할당
    /// </summary>
    /// <param name="bpShip"></param>
    public void SetBlueprint(BlueprintShip bpShip)
    {
        blueprintShip = bpShip;
    }

    /// <summary>
    /// 설계도 함선 호출
    /// </summary>
    /// <returns></returns>
    public BlueprintShip GetBlueprintShip()
    {
        return blueprintShip;
    }

    /// <summary>
    /// 그리드 배치 담당 오브젝트 세팅
    /// </summary>
    /// <param name="placer"></param>
    public void SetGridPlacer(GridPlacer placer)
    {
        gridPlacer = placer;
    }

    /// <summary>
    /// 마우스가 selectionUI 위에 있는지 확인
    /// </summary>
    /// <param name="rootUI"></param>
    /// <returns></returns>
    private bool IsPointerOverUIObject(GameObject rootUI)
    {
        PointerEventData eventData = new(EventSystem.current) { position = Input.mousePosition };

        List<RaycastResult> raycastResults = new();
        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
            if (result.gameObject != null && result.gameObject.transform.IsChildOf(rootUI.transform))
                return true;

        return false;
    }

    public int GetCost()
    {
        return bpRoomCost;
    }

    /// <summary>
    /// Blueprint Room이 점유하고 있는 타일 반환
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> GetOccupiedTiles()
    {
        return occupiedTiles;
    }

    /// <summary>
    /// BPRoom의 점유 타일 업데이트
    /// </summary>
    public void UpdateOccupiedTiles()
    {
        occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpRoomSize, bpRotation);
    }
}
