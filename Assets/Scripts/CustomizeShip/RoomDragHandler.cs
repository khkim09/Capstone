using UnityEngine;
using UnityEngine.EventSystems;

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
    /// 방 설치 시 미리보기용 프리팹입니다.
    /// </summary>
    public GameObject previewPrefab;

    /// <summary>
    /// 설치될 방의 크기입니다.
    /// </summary>
    [HideInInspector] public Vector2Int roomSize;

    /// <summary>
    /// 현재 회전 각도입니다.
    /// </summary>
    [HideInInspector] public int rotation;

    /// <summary>
    /// 설치될 방의 레벨 정보입니다.
    /// </summary>
    [HideInInspector] public int roomLevel;

    /// <summary>
    /// 프리뷰용 SpriteRenderer 컴포넌트입니다.
    /// </summary>
    [Header("Preview Settings")] public SpriteRenderer previewSR;

    /// <summary>
    /// 생성된 프리뷰 인스턴스입니다.
    /// </summary>
    private GameObject previewInstance;

    /// <summary>
    /// 드래그 중 여부입니다.
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// 배치된 방들의 부모 오브젝트입니다.
    /// </summary>
    private Transform placedRoomParent;

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
            return;

        if (Input.GetMouseButtonDown(1)) // 우클릭 때마다 시계 방향으로 90도 회전
        {
            Debug.Log("회전 90도");
            rotation = (rotation + 90) % 360;
            previewInstance.transform.Rotate(0, 0, -90);
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
        // roomPrefab = levelData.roomPrefab;
        // previewPrefab = levelData.previewPrefab;
    }

    /// <summary>
    /// 드래그 시작 시 호출됩니다.
    /// 프리뷰 생성 및 alpha 설정, z값 조정 처리.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        previewInstance = Instantiate(previewPrefab);
        previewSR = previewInstance.GetComponentInChildren<SpriteRenderer>();
        SetAlpha(previewInstance, 0.5f);
        MoveToFront(previewInstance);
    }

    /// <summary>
    /// 드래그 중 실시간 위치 이동 및 설치 가능 여부에 따라 프리뷰 색상 표시.
    /// 기준점은 좌측 하단입니다.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

        bool canPlace = GridPlacer.Instance.CanPlaceRoom(gridPos, roomSize);
        Color targetColor = canPlace ? Color.green : Color.red;
        targetColor.a = 0.5f;
        previewSR.color = targetColor;

        Vector3 basePos = ShipGridManager.Instance.GridToWorldPosition(gridPos);

        // 좌측 하단 블록 중심으로 세팅
        float dx = roomSize.x / 2.0f - 0.5f;
        float dy = roomSize.y / 2.0f - 0.5f;
        previewInstance.transform.position = basePos + new Vector3(dx, dy, 0);

        /*
                if (Input.GetMouseButtonDown(1)) // 우클릭 90도 회전
                {
                    Debug.Log("회전 90도");
                    rotation = (rotation + 90) % 360; // 시계 방향으로 90도씩 돌리면 rotation + 90 이 맞는지 (왜냐면 방 정보 저장할 때 rotation값도 저장해줘야 함선 호출 시 제대로 배치 가능)
                    previewInstance.transform.Rotate(0, 0, -90);
                }
        */
    }

    /// <summary>
    /// 드래그 종료 후 방을 설치하고 프리뷰를 제거합니다.
    /// 설치 성공 시 인벤토리를 갱신합니다.
    /// </summary>
    /// <param name="eventData">드래그 이벤트 데이터.</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        Vector2Int gridPos = ShipGridManager.Instance.WorldToGridPosition(worldPos);

        GameObject placedRoom = GridPlacer.Instance.PlaceRoom(gridPos, roomSize, roomPrefab, rotation);
        if (placedRoom != null)
        {
            var roomComp = placedRoom.GetComponent<PlacedRoomInteraction>();
            roomComp.roomData = roomData;
            roomComp.rotation = rotation;

            var sr = placedRoom.GetComponent<SpriteRenderer>();
            sr.sprite = roomData.GetRoomData(roomLevel).roomSprite;

            RoomsInventoryTooltipUI.Instance.RefreshInventory(); // 설치 후 인벤토리 갱신
        }
        Destroy(previewInstance);
    }

    /// <summary>
    /// 오브젝트의 alpha 값을 설정합니다.
    /// </summary>
    /// <param name="obj">대상 오브젝트.</param>
    /// <param name="alpha">적용할 alpha 값.</param>
    private void SetAlpha(GameObject obj, float alpha)
    {
        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            var c = sr.color;
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
