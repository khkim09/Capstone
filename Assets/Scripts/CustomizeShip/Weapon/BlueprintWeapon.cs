using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 설계도에 배치된 무기 정보.
/// </summary>
public class BlueprintWeapon : MonoBehaviour, IBlueprintPlaceable
{
    /// <summary>ShipWeaponData 참조</summary>
    public ShipWeaponData bpWeaponData;

    /// <summary>무기 부착 방향</summary>
    public ShipWeaponAttachedDirection bpAttachedDirection = ShipWeaponAttachedDirection.North;

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

    /// <summary>해당 무기의 설치 비용</summary>
    public int bpWeaponCost;

    /// <summary>무기 크기 (고정: 2x1)</summary>
    public Vector2Int bpWeaponSize = new(2, 1);

    /// <summary>
    /// 회전각 고정 (0)
    /// </summary>
    public Constants.Rotations.Rotation bpRotation = Constants.Rotations.Rotation.Rotation0;

    /// <summary>
    /// 실제 점유 타일
    /// </summary>
    public List<Vector2Int> occupiedTiles = new();

    /// <summary>
    /// 설계도 무기의 Sprite Renderer
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
    /// 이동 전 무기 위치
    /// </summary>
    private Vector2Int originalPos;

    /// <summary>
    /// 이동 전 무기 부착 방향
    /// </summary>
    private ShipWeaponAttachedDirection originalDirection;

    /// <summary>
    /// 그리드 타일 배치 작업을 위한 오브젝트
    /// </summary>
    private GridPlacer gridPlacer;

    /// <summary>
    /// 마우스로 클릭한 오브젝트 (무기 삭제 여부 활성화 조건을 위한 변수)
    /// </summary>
    private GameObject mouseDownTarget;

    /// <summary>
    /// 함선 외갑판 레벨 (0: 레벨 1, 1: 레벨 2, 2: 레벨 3)
    /// 이 변수는 단지 캐싱 용도로만 사용됨. 실제 레벨은 blueprintShip.GetHullLevel()에서 가져와야 함
    /// </summary>
    [SerializeField] private int hullLevel = 0;

    /// <summary>
    /// collider size 맞춤, 외갑판 레벨로 무기 디자인 업데이트
    /// </summary>
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Sprite sprite = sr.sprite;

        float width = sprite.rect.width / sprite.pixelsPerUnit;
        float height = sprite.rect.height / sprite.pixelsPerUnit;

        GetComponent<BoxCollider2D>().size = new Vector2(width, height);

        // 시작 시 함선의 외갑판 레벨로 업데이트
        if (blueprintShip != null)
        {
            SetHullLevel(blueprintShip.GetHullLevel());
            ApplyAttachedDirectionSprite();
        }
    }

    /// <summary>
    /// 무기 배치와 관련된 모든 작업을 매 프레임 검사
    /// </summary>
    private void Update()
    {
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // 회전 (무기는 부착 방향만 변경)
        if (isDragging && Input.GetMouseButtonDown(1))
        {
            // 드래그 중이면 회전 방어 조건
            if (BlueprintWeaponDragHandler.IsWeaponBeingDragged)
                return;

            if (hit.collider != null && hit.collider.gameObject == gameObject)
                RotateAttachedDirection();
        }

        // 드래그 시작 (좌클릭)
        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            if (BlockBPMovement())
            {
                isDragging = false;
                return;
            }

            if (IsPointerOverBlockingUI())
                return;

            if (EventSystem.current.IsPointerOverGameObject())
                if (IsPointerOverUIObject(RoomSelectionHandler.Instance.selectionUI))
                    return;

            if (RoomSelectionHandler.Instance != null)
                RoomSelectionHandler.Instance.Deselect();

            // 드래그 전 선택된 오브젝트 해제
            RoomSelectionHandler.Instance.Deselect();

            // 드래그 시작 시 정보 저장
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                originalPos = bpPosition;
                originalDirection = bpAttachedDirection;

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

            // 동일한 오프셋 적용
            Vector2Int size = new(2, 1);
            Vector2 offset = RoomRotationUtility.GetRotationOffset(size, Constants.Rotations.Rotation.Rotation0);
            transform.position = gridPlacer.GetWorldPositionFromGrid(bpPosition) + (Vector3)offset;

            // bool canPlace = gridPlacer.CanPlaceObject(this, bpPosition, null);
            bool canPlace = gridPlacer.CanPlaceWeapon(bpWeaponData, bpPosition, bpAttachedDirection);
            sr.color = canPlace ? validColor : invalidColor;
        }

        // 드래그 종료
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Vector2Int newPos = gridPlacer.WorldToGridPosition(mouseWorldPos);

            UpdateOccupiedTiles();

            bool canPlace = gridPlacer.CanPlaceWeapon(bpWeaponData, bpPosition, bpAttachedDirection);
            sr.color = Color.white;

            // 유효성 검사
            if (!canPlace)
            {
                // 불가능 : 원위치
                bpPosition = originalPos;
                bpAttachedDirection = originalDirection;
                bpWeaponSize = new Vector2Int(2, 1);

                UpdateOccupiedTiles();

                Vector2 offset =
                    RoomRotationUtility.GetRotationOffset(bpWeaponSize, Constants.Rotations.Rotation.Rotation0);
                transform.position = gridPlacer.GetWorldPositionFromGrid(originalPos) + (Vector3)offset;
                ApplyAttachedDirectionSprite();

                // 점유 타일 복구
                gridPlacer.MarkObjectOccupied(this);
                mouseDownTarget = null;
            }
            else
            {
                bpPosition = newPos;
                occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpWeaponSize, bpRotation);
                gridPlacer.MarkObjectOccupied(this);
            }

            transform.position += new Vector3(0, 0, 10f); // 뒤쪽으로 배치

            if (hit.collider != null && hit.collider.gameObject == mouseDownTarget)
                RoomSelectionHandler.Instance.SelectPlaceable(this);

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
                result.gameObject.name.Contains("Weapon") || result.gameObject.name.Contains("Hull"))
                return true;

        return false;
    }

    /// <summary>
    /// 설치 시 초기화
    /// </summary>
    public void Initialize(ShipWeaponData data, Vector2Int pos, ShipWeaponAttachedDirection direction)
    {
        occupiedTiles.Clear();

        bpWeaponData = data;
        bpPosition = pos;
        bpAttachedDirection = direction;
        bpWeaponCost = data.cost;
        bpWeaponSize = new Vector2Int(2, 1);

        occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpWeaponSize, bpRotation);

        sr = GetComponent<SpriteRenderer>();

        // 무기 데이터에서 스프라이트 설정
        ApplyAttachedDirectionSprite();
    }

    /// <summary>
    /// 드래그 모드 설정
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
    /// 무기 부착 방향 회전
    /// </summary>
    public void RotateAttachedDirection()
    {
        // 현재 attachedDirection 값을 기반으로 다음 상태로 전환
        switch (bpAttachedDirection)
        {
            case ShipWeaponAttachedDirection.East:
                bpAttachedDirection = ShipWeaponAttachedDirection.South;
                break;
            case ShipWeaponAttachedDirection.South:
                bpAttachedDirection = ShipWeaponAttachedDirection.North;
                break;
            case ShipWeaponAttachedDirection.North:
                bpAttachedDirection = ShipWeaponAttachedDirection.East;
                break;
        }

        // 변경된 attachedDirection에 따른 스프라이트 적용
        ApplyAttachedDirectionSprite();
    }

    /// <summary>
    /// 현재 회전 상태 반환
    /// </summary>
    public object GetRotation()
    {
        return bpAttachedDirection;
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

        // 함선이 할당되면 함선의 현재 외갑판 레벨로 업데이트
        if (blueprintShip != null)
        {
            SetHullLevel(blueprintShip.GetHullLevel());
            ApplyAttachedDirectionSprite();
        }
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

    /// <summary>
    /// 현재 부착 방향과 외갑판 레벨에 맞는 스프라이트 적용
    /// </summary>
    public void ApplyAttachedDirectionSprite()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        int directionIndex = 0;

        // 방향에 따른 인덱스 설정
        switch (bpAttachedDirection)
        {
            case ShipWeaponAttachedDirection.North:
                directionIndex = 0;
                break;
            case ShipWeaponAttachedDirection.East:
                directionIndex = 1;
                break;
            case ShipWeaponAttachedDirection.South:
                directionIndex = 2;
                break;
        }

        // 함선에서 외갑판 레벨 가져오기 (캐싱된 값이 아닌 실제 함선의 값 사용)
        // int currentHullLevel = blueprintShip != null ? blueprintShip.GetHullLevel() : hullLevel;
        int currentHullLevel = hullLevel;

        // 외갑판 레벨과 방향에 맞는 스프라이트 적용
        try
        {
            if (bpWeaponData.blueprintSprites != null &&
                bpWeaponData.blueprintSprites[currentHullLevel, directionIndex] != null)
            {
                sr.sprite = bpWeaponData.blueprintSprites[currentHullLevel, directionIndex];
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"무기 스프라이트 적용 중 오류 발생: {ex.Message}");
        }

        // 스프라이트가 없으면 아이콘으로 대체
        if (bpWeaponData.weaponIcon != null) sr.sprite = bpWeaponData.weaponIcon;
    }

    /// <summary>
    /// 설치 비용 반환
    /// </summary>
    public int GetCost()
    {
        return bpWeaponCost;
    }

    /// <summary>
    /// 함선 외갑판 레벨 설정 (내부적으로만 캐싱)
    /// </summary>
    /// <param name="level">외갑판 레벨 (0-2)</param>
    public void SetHullLevel(int level)
    {
        if (level >= 0 && level < 3)
        {
            hullLevel = level;
            ApplyAttachedDirectionSprite();
        }
        else
        {
            hullLevel = 0;
            ApplyAttachedDirectionSprite();
        }
    }

    /// <summary>
    /// 함선 외갑판 레벨 가져오기
    /// 실제로는 blueprintShip.GetHullLevel()을 우선적으로 사용해야 하며,
    /// 이 메서드는 blueprintShip이 null일 때의 레벨 값만 반환함
    /// </summary>
    /// <returns>외갑판 레벨 (0-2)</returns>
    public int GetHullLevel()
    {
        return blueprintShip != null ? blueprintShip.GetHullLevel() : hullLevel;
    }

    /// <summary>
    /// 점유 타일 목록 반환
    /// </summary>
    public List<Vector2Int> GetOccupiedTiles()
    {
        return occupiedTiles;
    }

    /// <summary>
    /// 점유 타일 목록 업데이트
    /// </summary>
    public void UpdateOccupiedTiles()
    {
        occupiedTiles = RoomRotationUtility.GetOccupiedGridPositions(bpPosition, bpWeaponSize, bpRotation);
    }
}
