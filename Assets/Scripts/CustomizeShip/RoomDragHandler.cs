using UnityEngine;
using UnityEngine.EventSystems;

// NOTE: 수정점 : 방 미리보기 및 방을 설치할 때 동적으로 AddComponent로 해당 방 속성을 달게 시켰음
//       나중에 도안 시스템을 반영할 때는 단순히 누르는 버튼(혹은 UI)에 따라 해당하는 Room 컴포넌트와 Roomdata를 추가시키면 될 것임
//       기본적으로 방을 동적으로 생성하는 구조다보니, 모든 RoomData Scriptable Object를 배열로 인스펙터에서 할당해야될 것으로 사료됨


/// <summary>
/// 방 설치, 회전, 삭제, 이동까지 담당하는 드래그 핸들러.
/// 방 프리뷰를 생성하고 배치 가능 여부를 시각적으로 표시합니다.
/// </summary>
public class RoomDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static RoomDragHandler Instance;

    /// <summary>
    /// 설치할 방의 데이터입니다.
    /// </summary>
    [Header("Room Info")] public RoomData roomData;

    /// <summary>
    /// 설치될 방의 프리팹입니다.
    /// </summary>
    public GameObject roomPrefab;

    /// <summary>
    /// 방 설치 시 미리보기용 프리팹입니다. (간단한 스프라이트만 있는 GameObject)
    /// </summary>
    public GameObject previewPrefab;

    /// <summary>
    /// 설치될 방의 크기입니다.
    /// </summary>
    [HideInInspector] public Vector2Int roomSize;

    /// <summary>
    /// 설치될 방의 레벨 정보입니다.
    /// </summary>
    [HideInInspector] public int roomLevel;

    /// <summary>
    /// 현재 회전 상태입니다.
    /// </summary>
    [HideInInspector] public RoomRotation currentRotation = RoomRotation.Rotation0;

    /// <summary>
    /// 생성된 프리뷰 인스턴스입니다.
    /// </summary>
    private GameObject previewInstance;

    /// <summary>
    /// 프리뷰의 스프라이트 렌더러입니다.
    /// </summary>
    private SpriteRenderer previewSpriteRenderer;

    /// <summary>
    /// 드래그 중 여부입니다.
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// 배치된 방들의 부모 오브젝트입니다.
    /// </summary>
    private Transform placedRoomParent;

    /// <summary>
    /// Awake 시 싱글턴 인스턴스를 설정합니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    /// <summary>
    /// 시작 시 PlacedRooms 오브젝트를 찾아 설정합니다.
    /// </summary>
    private void Start()
    {
        GameObject found = GameObject.Find("PlacedRooms");
        if (found != null)
            placedRoomParent = found.transform;
    }

    /// <summary>
    /// 드래그 중 우클릭 시 마다, 설치할 방의 실루엣을 시계 방향으로 90도 회전합니다.
    /// </summary>
    private void Update()
    {
        if (!isDragging || previewInstance == null)
        {
            Debug.Log("Not dragging or preview instance is null");
            return;
        }

        Debug.Log("Updatae");
        if (Input.GetMouseButtonDown(1)) // 우클릭 시 방 회전
        {
            // 프리뷰 객체 회전
            previewInstance.transform.Rotate(0, 0, 90);

            // 회전 상태 업데이트
            currentRotation = (RoomRotation)(((int)currentRotation + 1) % 4);


            // 회전 후 위치 재조정 및 배치 가능 여부 다시 확인
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

            // 회전된 크기 고려하여 배치 가능 여부 확인
            Vector2Int rotatedSize = GetRotatedSize(roomSize, (int)currentRotation);
            bool canPlace = GridPlacer.Instance.CanPlaceRoom(gridPos, rotatedSize);

            // 위치 설정
            Vector3 basePos = ShipGridManager.Instance.GridToWorldPosition(gridPos);
            float dx = rotatedSize.x / 2.0f - 0.5f;
            float dy = rotatedSize.y / 2.0f - 0.5f;
            previewInstance.transform.position = basePos + new Vector3(dx, dy, 0);

            // 색상 설정 (배치 가능 여부에 따라)
            Color previewColor = canPlace ? Color.green : Color.red;
            previewColor.a = 0.5f;
            previewSpriteRenderer.color = previewColor;
        }
    }

    /// <summary>
    /// 외부에서 전달된 방 데이터로 내부 정보 초기화.
    /// </summary>
    /// <param name="data">설치할 방 데이터.</param>
    /// <param name="level">설치할 방의 레벨.</param>
    public void InitializeFromRoomData(RoomData data, int level)
    {
        roomData = data;
        roomLevel = level;

        var levelData = data.GetRoomData(level);
        if (levelData == null)
        {
            Debug.LogError("Invalid RoomData or Level");
            return;
        }

        roomSize = levelData.size;
        currentRotation = RoomRotation.Rotation0; // 회전 초기화
    }

    /// <summary>
    /// 드래그 시작 시 호출됩니다.
    /// 간단한 프리뷰 객체를 생성하고 스프라이트만 설정합니다.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // 단순한 프리뷰 객체 생성
        previewInstance = Instantiate(previewPrefab);

        // 스프라이트 렌더러 가져오기
        previewSpriteRenderer = previewInstance.GetComponent<SpriteRenderer>();
        if (previewSpriteRenderer == null)
        {
            Debug.LogError("Preview prefab does not have SpriteRenderer!");
            Destroy(previewInstance);
            previewInstance = null;
            isDragging = false;
            return;
        }

        // 방 데이터에서 스프라이트 설정
        if (roomData != null)
        {
            previewSpriteRenderer.sprite = roomData.GetRoomData(roomLevel).roomSprite;
        }

        // 반투명하게 설정
        SetAlpha(previewInstance, 0.5f);
        MoveToFront(previewInstance);
    }

    /// <summary>
    /// 드래그 중 실시간 위치 이동 및 설치 가능 여부에 따라 프리뷰 색상 표시.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (previewInstance == null || previewSpriteRenderer == null)
            return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

        // 회전된 크기 고려하여 배치 가능 여부 확인
        Vector2Int rotatedSize = GetRotatedSize(roomSize, (int)currentRotation);
        bool canPlace = GridPlacer.Instance.CanPlaceRoom(gridPos, rotatedSize);

        // 위치 설정
        Vector3 basePos = ShipGridManager.Instance.GridToWorldPosition(gridPos);
        float dx = rotatedSize.x / 2.0f - 0.5f;
        float dy = rotatedSize.y / 2.0f - 0.5f;
        previewInstance.transform.position = basePos + new Vector3(dx, dy, 0);

        // 색상 설정 (배치 가능 여부에 따라)
        Color previewColor = canPlace ? Color.green : Color.red;
        previewColor.a = 0.5f;
        previewSpriteRenderer.color = previewColor;
    }

   /// <summary>
/// 드래그 종료 후 실제 Room 오브젝트를 생성하고 배치합니다.
/// </summary>
/// <param name="eventData">드래그 이벤트 데이터.</param>
public void OnEndDrag(PointerEventData eventData)
{
    isDragging = false;

    if (previewInstance == null)
        return;

    Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
    Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

    // 회전된 크기 고려
    Vector2Int rotatedSize = GetRotatedSize(roomSize, (int)currentRotation);
    bool canPlace = GridPlacer.Instance.CanPlaceRoom(gridPos, rotatedSize);

    if (canPlace)
    {
        // 실제 방 생성 위치 계산
        Vector2 centerOffset = new Vector2((rotatedSize.x - 1) * 0.5f, (rotatedSize.y - 1) * 0.5f);
        Vector3 roomPos = ShipGridManager.Instance.GridToWorldPosition(gridPos) + new Vector2(centerOffset.x, centerOffset.y);

        // 기본 GameObject 생성
        GameObject realRoom = Instantiate(roomPrefab, roomPos, Quaternion.identity, placedRoomParent);

        // Room 타입에 따른 컴포넌트 동적 추가
        Room roomComponent = AddAppropriateRoomComponent(realRoom, roomData);

        if (roomComponent != null)
        {
            // 해당 Room 컴포넌트에 RoomData 설정
            roomComponent.roomData = roomData;
            roomComponent.currentLevel = roomLevel;
            roomComponent.Initialize();

            // 회전 적용
            for (int i = 0; i < (int)currentRotation; i++)
            {
                roomComponent.RotateRoom(1);
            }

            // GridPlacer에 등록
            GridPlacer.Instance.RegisterPlacedRoom(gridPos, rotatedSize, roomComponent);

            // 위치 설정
            roomComponent.position = gridPos;

            // 배치 완료 이벤트 호출
            roomComponent.OnPlaced();
        }
        else
        {
            Debug.LogError("Failed to add appropriate Room component!");
            Destroy(realRoom);
        }

        // 인벤토리 갱신
        if (RoomsInventoryTooltipUI.Instance != null)
            RoomsInventoryTooltipUI.Instance.RefreshInventory();
    }

    // 프리뷰 삭제
    Destroy(previewInstance);
    previewInstance = null;
}

/// <summary>
/// 방 데이터에 따라 적절한 Room 컴포넌트를 추가합니다.
/// </summary>
/// <param name="roomObject">방 GameObject</param>
/// <param name="data">방 데이터</param>
/// <returns>추가된 Room 컴포넌트</returns>
private Room AddAppropriateRoomComponent(GameObject roomObject, RoomData data)
{
    // 방 데이터 타입에 따라 적절한 컴포넌트 추가
    Room roomComponent = null;

    if (data is EngineRoomData engineData)
    {
        var room = roomObject.AddComponent<EngineRoom>();
        // 엔진룸 특화 속성 설정
        room.roomData = engineData;
        roomComponent = room;
    }
    else if (data is PowerRoomData powerData)
    {
        var room = roomObject.AddComponent<PowerRoom>();
        // 전력실 특화 속성 설정
        room.roomData = powerData;
        roomComponent = room;
    }
    else if (data is CockpitRoomData cockpitData)
    {
        var room = roomObject.AddComponent<CockpitRoom>();
        // 조종실 특화 속성 설정
        room.roomData = cockpitData;
        roomComponent = room;
    }
    else if (data is OxygenRoomData oxygenData)
    {
        var room = roomObject.AddComponent<OxygenRoom>();
        // 산소실 특화 속성 설정
        room.roomData = oxygenData;
        roomComponent = room;
    }
    else if (data is MedBayRoomData medBayData)
    {
        var room = roomObject.AddComponent<MedBayRoom>();
        // 의무실 특화 속성 설정
        room.roomData = medBayData;
        roomComponent = room;
    }
    else if (data is ShieldRoomData shieldData)
    {
        var room = roomObject.AddComponent<ShieldRoom>();
        // 방어막실 특화 속성 설정
        room.roomData = shieldData;
        roomComponent = room;
    }
    else if (data is StorageRoomBaseData storageData)
    {
        // TODO: 창고는 창고별로 추가
        var room = roomObject.AddComponent<StorageRoomBase>();
        // 창고 특화 속성 설정
        room.roomData = storageData;
        roomComponent = room;
    }
    else if (data is CrewQuartersRoomData crewData)
    {
        var room = roomObject.AddComponent<CrewQuartersRoom>();
        // 선원실 특화 속성 설정
        room.roomData = crewData;
        roomComponent = room;
    }
    else
    {
        // 알 수 없는 타입은 기본 Room 컴포넌트 추가
        Debug.LogWarning("Unknown room data type: " + data.GetType().Name);
        roomComponent = roomObject.AddComponent<Room>();
        roomComponent.roomData = data;
    }

    return roomComponent;
}

    /// <summary>
    /// 회전에 따른 방의 크기를 반환합니다.
    /// </summary>
    private Vector2Int GetRotatedSize(Vector2Int originalSize, int rotation)
    {
        // 90도나 270도 회전 시 가로/세로 크기가 바뀜
        if (rotation % 2 == 1) // 90도 또는 270도
        {
            return new Vector2Int(originalSize.y, originalSize.x);
        }
        return originalSize;
    }

    /// <summary>
    /// 오브젝트의 alpha 값을 설정합니다.
    /// </summary>
    /// <param name="obj">대상 오브젝트.</param>
    /// <param name="alpha">적용할 alpha 값.</param>
    private void SetAlpha(GameObject obj, float alpha)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    /// <summary>
    /// 오브젝트의 z값을 조정해 UI보다 위에 표시되도록 합니다.
    /// </summary>
    /// <param name="obj">대상 오브젝트.</param>
    private void MoveToFront(GameObject obj)
    {
        obj.transform.position += new Vector3(0, 0, -0.5f);
    }
}
